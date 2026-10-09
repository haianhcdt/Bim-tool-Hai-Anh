using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Core.BatchTag;
using DSCons.Revit.Starter.Infrastructure;

namespace DSCons.Revit.Starter.Commands;

[Transaction(TransactionMode.Manual)]
public class BatchTagCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            var uiDoc = commandData.Application.ActiveUIDocument;
            if (uiDoc == null || uiDoc.Document == null)
            {
                message = "Không có mô hình Revit nào đang mở.";
                return Result.Cancelled;
            }

            var doc = uiDoc.Document;
            if (doc.IsReadOnly)
            {
                message = "Mô hình đang ở chế độ chỉ đọc (Read-only).";
                return Result.Failed;
            }

            var activeView = doc.ActiveView;
            if (activeView == null || activeView.IsTemplate)
            {
                message = "View hiện hành không hợp lệ hoặc là View Template.";
                return Result.Failed;
            }

            if (activeView.ViewType == ViewType.ThreeD)
            {
                var view3d = activeView as View3D;
                if (view3d != null && !view3d.IsLocked)
                {
                    TaskDialog.Show("BIM TOOL - Batch Tag",
                        "View 3D chưa được khóa hướng nhìn (Orientation Locked).\nVui lòng khóa View 3D hoặc chuyển sang Mặt bằng / Mặt cắt để gắn tag.");
                    return Result.Cancelled;
                }
            }

            // 1. Quét toàn bộ Ducts hiển thị trong Active View
            var ductsInView = new FilteredElementCollector(doc, activeView.Id)
                .OfCategory(BuiltInCategory.OST_DuctCurves)
                .WhereElementIsNotElementType()
                .Cast<Element>()
                .ToList();

            if (ductsInView.Count == 0)
            {
                TaskDialog.Show("BIM TOOL - Batch Tag",
                    $"Không tìm thấy đoạn ống gió nào hiển thị trong View '{activeView.Name}'.");
                return Result.Succeeded;
            }

            // 2. Quét các IndependentTag hiện có trong View để kiểm tra chống trùng (Idempotency)
            var existingTags = new FilteredElementCollector(doc, activeView.Id)
                .OfClass(typeof(IndependentTag))
                .Cast<IndependentTag>()
                .ToList();

            var alreadyTaggedIds = new HashSet<long>();
            foreach (var tag in existingTags)
            {
                try
                {
                    var taggedId = tag.TaggedLocalElementId;
                    if (taggedId != null && taggedId != ElementId.InvalidElementId)
                    {
                        alreadyTaggedIds.Add(taggedId.AsLong());
                    }
                }
                catch
                {
                    // Bỏ qua nếu tag không liên kết phần tử cục bộ
                }
            }

            // 3. Chuẩn bị danh sách Candidate
            var candidates = new List<BatchTagCandidate>();
            var ductMap = new Dictionary<long, Element>();

            foreach (var duct in ductsInView)
            {
                var locCurve = duct.Location as LocationCurve;
                if (locCurve != null && locCurve.Curve != null)
                {
                    var p0 = locCurve.Curve.GetEndPoint(0);
                    var p1 = locCurve.Curve.GetEndPoint(1);
                    var candidate = BatchTagCandidate.FromCurveEndpoints(
                        duct.Id.AsLong(),
                        "Ducts",
                        p0.X, p0.Y, p0.Z,
                        p1.X, p1.Y, p1.Z);
                    candidates.Add(candidate);
                    ductMap[duct.Id.AsLong()] = duct;
                }
            }

            // 4. Lọc các ống chưa có tag
            var untagged = BatchTagFilter.FilterUntagged(candidates, alreadyTaggedIds);

            var result = new BatchTagResult
            {
                TotalFound = ductsInView.Count,
                AlreadyTagged = ductsInView.Count - untagged.Count,
                NewlyTagged = 0,
                Failed = 0
            };

            if (untagged.Count == 0)
            {
                TaskDialog.Show("BIM TOOL - Batch Tag", result.BuildSummaryMessage("ống gió"));
                return Result.Succeeded;
            }

            // 5. Mở Transaction an toàn
            using (var tx = new Transaction(doc, "BIM TOOL - Batch Tag Elements"))
            {
                var status = tx.Start();
                if (status != TransactionStatus.Started)
                {
                    message = "Không thể bắt đầu Transaction trong Revit.";
                    return Result.Failed;
                }

                foreach (var item in untagged)
                {
                    try
                    {
                        if (!ductMap.TryGetValue(item.ElementId, out var duct)) continue;

                        var midPoint = new XYZ(item.MidX, item.MidY, item.MidZ);
                        var reference = new Reference(duct);

                        var newTag = IndependentTag.Create(
                            doc,
                            activeView.Id,
                            reference,
                            false,
                            TagMode.TM_ADDBY_CATEGORY,
                            TagOrientation.Horizontal,
                            midPoint);

                        if (newTag != null)
                        {
                            newTag.TagHeadPosition = midPoint;
                            result.NewlyTagged++;
                        }
                        else
                        {
                            result.Failed++;
                        }
                    }
                    catch
                    {
                        result.Failed++;
                    }
                }

                tx.Commit();
            }

            TaskDialog.Show("BIM TOOL - Batch Tag", result.BuildSummaryMessage("ống gió"));
            Log.Write($"BatchTag: {result.NewlyTagged} newly tagged, {result.AlreadyTagged} skipped.");
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            Log.Write($"BatchTag error: {ex}");
            message = ex.Message;
            return Result.Failed;
        }
    }
}