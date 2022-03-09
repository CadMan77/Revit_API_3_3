//Создайте приложение, которое позволяет выбрать трубы, а также записывает их длину в метрах,
//увеличенную на коэффициент 1.1, в (заблаговременно) созданный (, и связанный с категорией Pipe общий)
//параметр («Длина с запасом», тип Длина).

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_API_3_3
{
    [Transaction(TransactionMode.Manual)]

    public class Main : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            //var categorySet = new CategorySet();
            //categorySet.Insert(Category.GetCategory(doc, BuiltInCategory.OST_PipeCurves));

            uidoc.RefreshActiveView();

            try
            {

                IList<Reference> selectedElementRefList = uidoc.Selection.PickObjects(ObjectType.Element, new PipeFilter(), "Выберите трубы:");

                if (selectedElementRefList.Count > 0)
                {

                    var pipeList = new List<Element>();
                    double pipeLength = 0, pipeLenMeter=0, stockLenMeter=0;

                    foreach (var selectedRef in selectedElementRefList)
                    {
                        Pipe oPipe = doc.GetElement(selectedRef) as Pipe;

                        pipeList.Add(oPipe);
                        //pipeLength = oPipe.LookupParameter("Length").AsDouble();
                        pipeLength = oPipe.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                        pipeLenMeter = Math.Round(UnitUtils.ConvertFromInternalUnits(pipeLength, UnitTypeId.Meters), 2);
                        stockLenMeter = pipeLenMeter*1.1;

                        using (Transaction ts = new Transaction(doc, "Set Shared Parameter Value"))
                        {
                            ts.Start();
                            //Parameter commentsParameter = oPipe.LookupParameter.LookupParameter("Comments");
                            Parameter dlinaZapasomParameter = oPipe.LookupParameter("Длина с запасом");
                            dlinaZapasomParameter.Set(stockLenMeter);
                            ts.Commit();
                        }                   
                    }

                    TaskDialog.Show("Результат", $"Труб обработано - {pipeList.Count}");
                }
            }
            catch { }
            return Result.Succeeded;
        }
    }
}