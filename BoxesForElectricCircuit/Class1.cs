using System;
using System.Text;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.Creation;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Windows.Forms;
using Document = Autodesk.Revit.DB.Document;

namespace BoxesForElectricCircuit
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Program : IExternalCommand
    {
        List<ElectricalSystem> ElectricalSystems = new List<ElectricalSystem>();
        List<Element> ConduitTypes = new List<Element>();
        List<Element> ConduitFitting = new List<Element>();

        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            try
            {
                var document = revit.Application.ActiveUIDocument.Document;
                GetSomeShit(document, out string error);

                if (error != null)
                {
                    TaskDialog.Show("Ошибка", error);
                    return Result.Succeeded;
                }

                var systemsByEquipmentIds = new Dictionary<ElementId, List<ElementId>>();

                foreach (var system in ElectricalSystems)
                {
                    var equipment = system.BaseEquipment;
                    if (equipment == null)
                        continue;
                    if (!systemsByEquipmentIds.ContainsKey(equipment.Id))
                        systemsByEquipmentIds.Add(equipment.Id, new List<ElementId>());
                    systemsByEquipmentIds[equipment.Id].Add(system.Id);
                }

                var conduitTypes = new Dictionary<string, ElementId>();
                foreach (ConduitType type in ConduitTypes)
                    conduitTypes.Add(type.FamilyName + ": " + type.Name, type.Id);

                var gui = new UserForm(document, systemsByEquipmentIds, conduitTypes);

                if (gui.ShowDialog() != DialogResult.OK)
                    return Result.Cancelled;

                var conduitTypeId = gui.SelectedConduitType;
                var electricalCircuitsWithLevelId = new List<Tuple<ElementId, List<XYZ>>>();
                foreach (var systemId in gui.SelectedCircuits)
                {
                    var system = (ElectricalSystem) document.GetElement(systemId);
                    var circuit = system.GetCircuitPath().ToList();
                    electricalCircuitsWithLevelId.Add(new Tuple<ElementId, List<XYZ>>(system.LevelId, circuit));
                }
                
                Transaction tr = new Transaction(document, "Создание коробов по электрической цепи");
                tr.Start();

                var delta = 0.0;

                foreach (ConduitType type in ConduitTypes)
                {
                    var dummyConduit = Conduit.Create(
                        document,
                        type.Id,
                        new XYZ(),
                        new XYZ(1, 1, 1),
                        electricalCircuitsWithLevelId.First().Item1);
                    delta = Math.Max(0, dummyConduit.Diameter);
                    document.Delete(dummyConduit.Id);
                }

                foreach (var x in electricalCircuitsWithLevelId)
                {
                    var createdConduits = new List<Conduit>();
                    for (var i = 0; i < x.Item2.Count() - 1; i++)
                    {
                        DeleteConduit(document, x.Item2.ElementAt(i), x.Item2.ElementAt(i + 1), delta);
                        var beginning = x.Item2.ElementAt(i);
                        var end = x.Item2.ElementAt(i + 1);
                        var levelId = x.Item1;
                        var conduit = Conduit.Create(document, conduitTypeId, beginning, end, levelId);
                        createdConduits.Add(conduit);
                    }
                    var number = 1;
                    for (var i = 0; i < createdConduits.Count() - 1; i++)
                    {
                        var point = x.Item2.ElementAt(number);
                        Connect(point,
                            createdConduits.ElementAt(i),
                            createdConduits.ElementAt(i + 1));
                        number++;
                    }
                }
                
                tr.Commit();
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.ToString());
                return Result.Failed;
            }
        }

        private void GetSomeShit(Document document, out string message)
        {
            var collector = new FilteredElementCollector(document);
            var electricalEquipment = collector
                .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                .OfClass(typeof(FamilySymbol))
                .ToElements()
                .ToList();
            if (electricalEquipment.Count() == 0)
            {
                message = "В текущем проетке нет типов электрооборудывания!";
                return;
            }

            collector = new FilteredElementCollector(document);
            ElectricalSystems = collector
                .OfClass(typeof(ElectricalSystem))
                .ToElements()
                .Select(x => (ElectricalSystem)x)
                .ToList();
            if (!CheckElectricalSystems(ElectricalSystems))
            {
                message = "В текущем проетке нет электрических цепей!";
                return;
            }

            collector = new FilteredElementCollector(document);
            ConduitTypes = collector
                .OfClass(typeof(ConduitType))
                .ToElements()
                .ToList();
            if (ConduitTypes.Count == 0)
            {
                message = "В текущем проетке нет типов коробов!";
                return;
            }

            collector = new FilteredElementCollector(document);
            ConduitFitting = collector
                .OfCategory(BuiltInCategory.OST_ConduitFitting)
                 .OfClass(typeof(FamilySymbol))
                .ToElements()
                .ToList();
            if (ConduitFitting.Count() == 0)
            {
                message = "В текущем проетке нет типов соеденительных деталей коробов!";
                return;
            }
            message = null;
        }

        private bool CheckElectricalSystems(IEnumerable<ElectricalSystem> electricalSystems)
        {
            foreach (ElectricalSystem system in electricalSystems)
            {
                var circuit = system.GetCircuitPath();
                if (circuit.Count > 0)
                    return true;
            }
            return false;
        }

        static Connector GetConnectorClosestTo( ConnectorSet connectors, XYZ point)
        {
            Connector targetConnector = null;
            double minDist = double.MaxValue;

            foreach (Connector connector in connectors)
            {
                double distance = connector.Origin.DistanceTo(point);

                if (distance < minDist)
                {
                    targetConnector = connector;
                    minDist = distance;
                }
            }
            return targetConnector;
        }

        static ConnectorManager GetConnectorManager(Element element)
        {
            MEPCurve mepCurve = element as MEPCurve;
            FamilyInstance familyInstance = element as FamilyInstance;

            if (null == mepCurve && null == familyInstance)
            {
                throw new ArgumentException(
                  "Element is neither an MEP curve nor a fitting.");
            }

            return null == mepCurve
              ? familyInstance.MEPModel.ConnectorManager
              : mepCurve.ConnectorManager;
        }

        public static void Connect(XYZ point, Element firstElement, Element secondElement)
        {
            ConnectorManager connectorManager = GetConnectorManager(firstElement);
            if (null == connectorManager)
                throw new ArgumentException($"Element with id({firstElement.Id}) has no connectors.");
            Connector firstElementConnector = GetConnectorClosestTo(connectorManager.Connectors, point);

            connectorManager = GetConnectorManager(secondElement);
            if (null == connectorManager)
                throw new ArgumentException($"Element with id({secondElement.Id}) has no connectors.");
            Connector secondElementConnector = GetConnectorClosestTo(connectorManager.Connectors, point);

            firstElementConnector.ConnectTo(secondElementConnector);
        }

        private void DeleteConduit(Document document, XYZ firstPoint, XYZ secondPoint, double delta)
        {
            var startPoint = new XYZ(
                Math.Min(firstPoint.X, secondPoint.X) - delta,
                Math.Min(firstPoint.Y, secondPoint.Y) - delta,
                Math.Min(firstPoint.Z, secondPoint.Z) - delta);
            var endPoint = new XYZ(
                Math.Max(firstPoint.X, secondPoint.X) + delta,
                Math.Max(firstPoint.Y, secondPoint.Y) + delta,
                Math.Max(firstPoint.Z, secondPoint.Z) + delta);
            var myOutLn = new Outline(startPoint, endPoint);
            var conduits = new List<ElementId>();
            if (!myOutLn.IsEmpty)
            {
                var filter = new BoundingBoxIsInsideFilter(myOutLn);
                var collector = new FilteredElementCollector(document);
                conduits = collector
                    .WherePasses(filter)
                    .OfClass(typeof(Conduit))
                    .Cast<Conduit>()
                    .Select(a => a.Id)
                    .ToList();
            }
            document.Delete(conduits);
        }

        private void CutCondiut(
            double delta,
            XYZ originalBeginning,
            XYZ originalEnd,
            out XYZ resultBeginning,
            out XYZ resultEnd)
        {
            var vector = new XYZ(
                originalEnd.X - originalBeginning.X,
                originalEnd.Y - originalBeginning.Y,
                originalEnd.Z - originalBeginning.Z);
            var vectorLen = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
            var k = (vectorLen - delta) / vectorLen;
            var tempVector = new XYZ(
                vector.X * k,
                vector.Y * k,
                vector.Z * k);
            resultEnd = new XYZ(
                originalBeginning.X + tempVector.X,
                originalBeginning.Y + tempVector.Y,
                originalBeginning.Z + tempVector.Z);
            resultBeginning = new XYZ(
                originalEnd.X - tempVector.X,
                originalEnd.Y - tempVector.Y,
                originalEnd.Z - tempVector.Z);
        }
    }
}
