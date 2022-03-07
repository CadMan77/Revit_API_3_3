//Создайте приложение, которое позволяет выбрать трубы, а также записывает их длину в метрах,
//увеличенную на коэффициент 1.1, в созданный (заблаговременно, общий) параметр («Длина с запасом», тип Длина).

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
                    double totLength = 0, totLenMeter=0, orderLenMeter=0;

                    foreach (var selectedRef in selectedElementRefList)
                    {
                        Pipe oPipe = doc.GetElement(selectedRef) as Pipe;
                        pipeList.Add(oPipe);
                        //totLength += oPipe.LookupParameter("Length").AsDouble();
                        totLength += oPipe.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                    }
                    totLenMeter = Math.Round(UnitUtils.ConvertFromInternalUnits(totLength, UnitTypeId.Meters), 2);
                    orderLenMeter = totLenMeter*1.1;
                    //TaskDialog.Show("Результат", $"Труб выбрано - {pipeList.Count}{Environment.NewLine}Общая длина выборки - {totLenMeter} м{Environment.NewLine}Длина с запасом (10%) - {totLenMeter} м"); 

                    using (Transaction ts = new Transaction(doc, "Set Shared Parameter Value"))
                    {
                        ts.Start();
                        //Parameter commentsParameter = doc.GetElement(selectedElementRefList.First()).LookupParameter("Comments");
                        //commentsParameter.Set(orderLenMeter);
                        Parameter dlinaZapasomParameter = doc.GetElement(selectedElementRefList.First()).LookupParameter("Длина с запасом");
                        dlinaZapasomParameter.Set(orderLenMeter);
                        ts.Commit();
                    }
                }
            }
            catch { }
            return Result.Succeeded;
        }
    }
}