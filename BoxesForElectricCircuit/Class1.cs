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
                
                foreach (var x in electricalCircuitsWithLevelId)
                {
                    var createdConduits = new List<Conduit>();
                    for (var i = 0; i < x.Item2.Count() - 1; i++)
                    {
                        var startPoint = new XYZ(
                            Math.Min(x.Item2[i].X, x.Item2[i + 1].X) - 0.01,
                            Math.Min(x.Item2[i].Y, x.Item2[i + 1].Y) - 0.01,
                            Math.Min(x.Item2[i].Z, x.Item2[i + 1].Z) - 0.01
                            );
                        var endPoint = new XYZ(
                            Math.Max(x.Item2[i].X, x.Item2[i + 1].X) + 0.01,
                            Math.Max(x.Item2[i].Y, x.Item2[i + 1].Y) + 0.01,
                            Math.Max(x.Item2[i].Z, x.Item2[i + 1].Z) + 0.01
                            );
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
                        var conduit = Conduit.Create(document, conduitTypeId, x.Item2.ElementAt(i), x.Item2.ElementAt(i + 1), x.Item1);
                        createdConduits.Add(conduit);
                    }

                    //for (var i = 0; i < createdConduits.Count() - 1; i++)
                    //{
                    //    var connector1 = new Connector();
                    //    var connector2;
                    //    foreach (Connector c in createdConduits.ElementAt(i).ConnectorManager.Connectors)
                    //    {
                    //        // моя тут сломался фмгня получается
                    //        connector1 = c;
                    //        break;
                    //    }
                    //    foreach (Connector c in createdConduits.ElementAt(i+1).ConnectorManager.Connectors)
                    //    {
                    //        
                    //        connector2 = c;
                    //        break;
                    //    }
                    //    document.Create.NewElbowFitting(
                    //        connector1,
                    //        connector2);
                    //}
                }
                
                tr.Commit();
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.ToString());
                return Result.Failed;
            }
            return Result.Succeeded;
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
    }
}
