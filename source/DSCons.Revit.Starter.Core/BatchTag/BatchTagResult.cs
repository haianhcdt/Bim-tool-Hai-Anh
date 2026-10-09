namespace DSCons.Revit.Starter.Core.BatchTag;

public sealed class BatchTagResult
{
    public int TotalFound { get; set; }
    public int AlreadyTagged { get; set; }
    public int NewlyTagged { get; set; }
    public int Failed { get; set; }

    public string BuildSummaryMessage(string category = "Ống gió")
    {
        if (TotalFound == 0)
        {
            return $"Không tìm thấy đối tượng {category} nào trong View hiện hành.";
        }

        if (NewlyTagged == 0 && AlreadyTagged > 0)
        {
            return $"Tất cả {AlreadyTagged} đối tượng {category} trong View đều đã có tag từ trước.\nKhông tạo thêm tag để tránh đè nét.";
        }

        var msg = $"Đã gắn tag thành công cho {NewlyTagged} đối tượng {category}!";
        if (AlreadyTagged > 0)
        {
            msg += $"\n(Bỏ qua {AlreadyTagged} đối tượng đã có sẵn tag)";
        }
        if (Failed > 0)
        {
            msg += $"\n(Có {Failed} đối tượng không thể gắn tag)";
        }
        return msg;
    }
}