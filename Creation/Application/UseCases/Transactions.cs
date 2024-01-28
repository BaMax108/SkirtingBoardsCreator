using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using SkirtingBoardsCreator.Creation.Controllers;
using SkirtingBoardsCreator.Creation.Domain.Interfaces;
using System;
using System.Diagnostics;

namespace SkirtingBoardsCreator.Creation.Application.UseCases
{
    internal class Transactions
    {
        readonly ISkBoardsController Controller;
        public Transactions(ISkBoardsController controller) => Controller = controller;

        public Result FacingFlip(FamilyInstance instance)
        {
            if (instance == null || instance.CanFlipFacing == false)
            { 
                return Result.Cancelled;
            }
            else
            {
                using (Transaction t = new Transaction(Controller.Doc, "Симметрия"))
                {
                    t.Start();
                    instance.flipFacing();
                    try
                    { 
                        t.Commit();
                    }
                    catch (Exception ex) { Debug.WriteLine(ex.Message); }
                }
                return Result.Succeeded;
            } 
        }

        public Result ChangeParameters(FamilyInstance instance, IParameterForChanging[] parameters, bool hasFurniture) 
        {
            if (instance == null || parameters == null || parameters.Length == 0)
            {
                return Result.Cancelled;
            }
            else
            {
                if (hasFurniture)
                {
                    using (Transaction t = new Transaction(Controller.Doc, "Расстановка фурнитуры"))
                    {
                        t.Start();
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            instance.LookupParameter(parameters[i].Name).Set((int)parameters[i].Value);
                        }
                        t.Commit();
                    }
                }
                return Result.Succeeded;
            }
        }

        public Result ChangingParam(FamilyInstance instance, BuiltInParameter bip, string value)
        {
            if (instance != null & value != null & value != string.Empty & value != "0")
            {
                Parameter parameter = instance.get_Parameter(bip);
                
                if (parameter != null)
                { 
                    using (Transaction t = new Transaction(Controller.Doc, "Редактирование атрибутов"))
                    {
                        t.Start();
                        switch (bip)
                        {
                            case BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS:
                                parameter.Set(value);
                                break;
                            default: 
                                parameter.SetValueString(value);
                                break;
                        }
                        t.Commit();
                    }
                    return Result.Succeeded;
                }
            }
            return Result.Cancelled;
        }

        public Result CreateInstance(out FamilyInstance instance, Line line, FamilySymbol symbol, Level lvl)
        {
            if (lvl == null || line == null || symbol == null || Math.Round(line.Length, 3) == 0)
            {
                instance = default;
                return Result.Cancelled;
            }

            Result result;
            using (Transaction t = new Transaction(Controller.Doc, "Плинтус"))
            {
                t.Start();
                if (symbol.IsActive == false) { symbol.Activate(); }

                instance = Controller.Doc.Create.NewFamilyInstance(
                    CreateTransformLine(line),
                    symbol,
                    lvl,
                    StructuralType.NonStructural);

                t.Commit();
                
                if (instance.IsValidObject)
                {
                    result = Result.Succeeded;
                }
                else
                { 
                    instance = default;
                    result = Result.Cancelled;
                }
            }

            return result;
        }

        private Line CreateTransformLine(Line line) =>
            line.CreateTransformed(new Transform((Controller.PickedElement as RevitLinkInstance)
                .GetTotalTransform())) as Line;
    }
}
