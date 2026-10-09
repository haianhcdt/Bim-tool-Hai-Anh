# BUG REPORT — Sửa lỗi trùng đè thẻ ghi chú khi căn chỉnh tag (Align Tags Overlap)

> NGUỒN CHÂN LÝ khi debug. Điều tra & viết báo cáo này TRƯỚC, DỪNG xin duyệt rồi mới sửa.
> Sửa tối thiểu, đúng gốc rễ. Luật "Quá tam ba bận": 3 lần không xong → đổi cách, báo người.

## Status
`RESOLVED`

## 1. Mô tả
- **Hiện tượng:** Sau khi chạy công cụ căn chỉnh thẻ ghi chú (`Align Tags`), các thẻ tag được gióng về cùng một tọa độ trên trục gióng (cùng Y nếu căn ngang, cùng X nếu căn dọc). Nếu các tag ban đầu có vị trí trên trục còn lại quá gần nhau hoặc trùng nhau, các tag sau khi căn chỉnh sẽ **nằm đè khít lên nhau**, chữ bị chồng lấn gây lỗi hiển thị bản vẽ kỹ thuật.
- **Mức độ ảnh hưởng:** Trung bình - Nghiệp vụ hiển thị bản vẽ (Ảnh hưởng trực tiếp đến chất lượng hồ sơ in ấn và thẩm mỹ trình bày mô hình MEP).

## 2. Các bước tái hiện
1. Mở mô hình Revit `HA_MODEL_STUDY.rvt` tại View mặt bằng `Typical Room WSHP` (tỷ lệ 1:100).
2. Chọn từ 2 thẻ ghi chú (`Duct Tags`, ví dụ các ID `877737` và `877740`) nằm trên các đoạn ống gió song song gần nhau hoặc cùng trục.
3. Nhấp nút **Align Tags** trên Ribbon panel `Revit API Kit`.
4. Trong cửa sổ xem trước (Preview), chọn chế độ **Phương ngang (Cùng Y)** hoặc **Phương dọc (Cùng X)** rồi bấm **Thực hiện căn chỉnh**.
5. Quan sát trên màn hình Revit: các thẻ tag thẳng hàng nhưng bị đè chồng lên nhau do không có khoảng cách an toàn tối thiểu giữa các tag.

## 3. Thực tế vs Kỳ vọng
- **Thực tế (Actual):** Các tag bị gióng về cùng trục nhưng tọa độ trục còn lại giữ nguyên giá trị cũ, dẫn đến các tag có khoảng cách nhỏ hơn kích thước nhãn sẽ bị đè chữ lên nhau hoàn toàn.
- **Kỳ vọng (Expected):** Sau khi sửa, các tag **vừa thẳng hàng** trên trục gióng, **vừa không bị trùng đè** (tự động phát hiện va chạm và giãn cách một khoảng cách an toàn tối thiểu `minSpacing` giữa các tag).

## 4. Ngữ cảnh
- **Phiên bản phần mềm chủ:** Autodesk Revit 2019 (Target Framework: `net47`, .NET Framework 4.7).
- **Mô hình kiểm thử:** `HA_MODEL_STUDY.rvt` (Đã lưu điểm lưu checkpoint Git an toàn: tag `checkpoint-ngay-5-pre-fix`).
- **Khảo sát chỉ-đọc qua Revit MCP (Snapshot bằng chứng thật):**
  - Active Document: `HA_MODEL_STUDY` (`modified: true`).
  - Active View: `Typical Room WSHP` (FloorPlan, ID: `709705`, Scale: 100).
  - Đối tượng đang chọn (Selection): Element ID `877737` và `877740`.
  - Chi tiết đối tượng MCP:
    - Element 877737: Class `IndependentTag`, Category `Duct Tags`, Type `M_Bottom Elevation Duct Tag` (Type ID: `607409`), Orientation `Horizontal`, Leader `No`.
    - Element 877740: Class `IndependentTag`, Category `Duct Tags`, Type `M_Bottom Elevation Duct Tag` (Type ID: `607409`), Orientation `Horizontal`, Leader `No`.

## 5. Phân tích nguyên nhân gốc (Root Cause Analysis)
- **Vị trí nghi ngờ:** `source/DSCons.Revit.Starter.Core/BatchTag/TagAlignmentCalculator.cs:45-68`
- **Phân tách RÕ RÀNG giữa Quan sát có căn cứ và Suy luận:**
  - **Quan sát có căn cứ (Factual Evidence từ Code):**
    - Trong hàm `CalculateAlignment`:
      - Khi `direction == TagAlignmentDirection.Horizontal`:
        ```csharp
        item.NewX = item.CurrentX; // Giữ nguyên vị trí X ban đầu
        item.NewY = targetY;        // Ép toàn bộ về cùng Y
        ```
      - Khi `direction == TagAlignmentDirection.Vertical`:
        ```csharp
        item.NewX = targetX;        // Ép toàn bộ về cùng X
        item.NewY = item.CurrentY; // Giữ nguyên vị trí Y ban đầu
        ```
    - Thuật toán ban đầu hoàn toàn không có bước kiểm tra va chạm (Collision Detection) hay điều chỉnh khoảng cách tối thiểu giữa các tag kề nhau trên trục phân bố.
  - **Suy luận kỹ thuật (Inference):**
    - Với nhãn ống gió `M_Bottom Elevation Duct Tag` ở tỷ lệ 1:100, chiều dài một khung text nhãn tương đương khoảng 2.0 – 3.0 feet trong không gian mô hình Revit.
    - Khi các tag có khoảng cách nhỏ hơn ngưỡng này, việc ép về cùng trục $Y$ hoặc $X$ tất yếu tạo ra va chạm đè chữ.

## 6. Phương án sửa đề xuất (Chọn 1, tối thiểu)
- [x] **Phương án A (Khuyến nghị — Sửa tối thiểu trong Core):**
  - Mở rộng thuật toán lõi `TagAlignmentCalculator.CalculateAlignment` với tham số `minSpacing` (mặc định khoảng cách an toàn, ví dụ `2.5 feet` trong Revit API units).
  - Sau khi tính toán tọa độ trục gióng chính, sắp xếp danh sách tag tăng dần theo trục phụ (X nếu căn ngang, Y nếu căn dọc).
  - Lặp qua từng tag kề nhau: Nếu `next.Pos - prev.Pos < minSpacing`, tự động điều chỉnh `next.Pos = prev.Pos + minSpacing`.
  - Ưu điểm: Sửa tập trung vào thuật toán độc lập trong `Starter.Core`, viết unit test xác minh 100% tự động, không can thiệp sâu vào Revit API UI phức tạp.
- [ ] **Phương án B (Thêm tùy chọn Spacing trên giao diện WPF):**
  - Bổ sung ô nhập `Khoảng cách giãn tag (mm)` trên giao diện `AlignTagsWindow.xaml` và ViewModel.
  - Nhược điểm: Phải sửa thêm XAML và ViewModel, rủi ro làm phình to phạm vi can thiệp ngày 5.

## 7. Kế hoạch xác minh
- [x] **Ca 1 (Ca lỗi cũ — Overlapping Collision Case):**
  - Đầu vào: 2 tag có tọa độ gần trùng nhau: Tag 1 `(X=10.0, Y=20.0)` và Tag 2 `(X=10.5, Y=35.0)`.
  - Căn ngang (cùng Y=20.0), `minSpacing = 3.0`.
  - Kết quả cũ: Tag 1 `(10.0, 20.0)`, Tag 2 `(10.5, 20.0)` → Trùng đè!
  - Kết quả mới: Tag 1 `(10.0, 20.0)`, Tag 2 `(13.0, 20.0)` → Thẳng hàng ngang $Y=20.0$, cách nhau $ge 3.0$, không trùng đè (Đã đạt qua Unit Test).
- [x] **Ca 2 (Ca bình thường — Normal Non-collision Case):**
  - Đầu vào: 2 tag có khoảng cách đủ rộng: Tag 1 `(X=10.0, Y=20.0)` và Tag 2 `(X=25.0, Y=35.0)`.
  - Căn ngang (cùng Y=20.0), `minSpacing = 3.0`.
  - Kết quả mới: Tag 1 `(10.0, 20.0)`, Tag 2 `(25.0, 20.0)` → Thẳng hàng ngang $Y=20.0$, giữ nguyên vị trí tự nhiên vì khoảng cách $15.0 ge 3.0$ (Đã đạt qua Unit Test).
- [x] Chạy kiểm thử tự động toàn bộ Unit Tests trong `DSCons.Revit.Starter.Core.Tests` đạt **20/20 Pass (100%)**.

## 8. Fix đã áp dụng (RESOLVED)
- **Thay đổi mã nguồn:**
  - [TagAlignmentCalculator.cs](file:///C:/Users/DELL/OneDrive/02.STUDY/12.REVIT AI/Projects/BIM-Tool-WorkingView/source/DSCons.Revit.Starter.Core/BatchTag/TagAlignmentCalculator.cs): Bổ sung tham số `double minSpacing = 0.0` và thuật toán phân tách va chạm chống đè chữ tự động trên trục phân bố (Horizontal & Vertical).
  - [AlignTagsViewModel.cs](file:///C:/Users/DELL/OneDrive/02.STUDY/12.REVIT AI/Projects/BIM-Tool-WorkingView/source/ViewModels/AlignTagsViewModel.cs): Thiết lập tham số mặc định an toàn `minSpacing: 2.5` feet cho bảng xem trước (Preview DataGrid) trước khi gửi vị trí sang Transaction.
  - [TagAlignmentTests.cs](file:///C:/Users/DELL/OneDrive/02.STUDY/12.REVIT AI/Projects/BIM-Tool-WorkingView/source/DSCons.Revit.Starter.Core.Tests/TagAlignmentTests.cs): Bổ sung 3 unit tests mới bao phủ ca trùng tọa độ, ca khoảng cách tự nhiên và ca căn dọc.
- **Kết quả kiểm thử:**
  - **Unit Tests:** 20/20 passed (100% Đạt, 0 Failed, thời gian: 88ms).
  - **Biên dịch Revit 2019 (`net47`):** `DSCons.Revit.Starter.dll` đạt **0 Error, 0 Warning**. Hot Reload sẵn sàng nạp ngay lập tức vào phiên Revit mà không cần khởi động lại.
