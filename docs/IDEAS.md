# Bảng phân loại 10 ý tưởng tự động hóa Revit MEP

Tài liệu lưu trữ các ý tưởng cải tiến quy trình làm việc trong Revit cho kỹ sư Cơ Điện, được phân loại từ cơ bản đến phức tạp theo chuẩn của Bộ Kit.

## Bảng phân loại chi tiết (Từ Cơ bản đến Phức tạp)

| STT | Tên ý tưởng | Mức độ | Khả thi với Revit API | Rủi ro chính | Cách thu nhỏ (Phạm vi tối thiểu - MVP) | Gợi ý làm vào ngày nào |
| :---: | :--- | :---: | :--- | :--- | :--- | :---: |
| **2** | **Tạo view làm việc trên Project Browser** | **CƠ BẢN** | **KHẢ THI**<br>ViewPlan, View3D, Parameter | Đặt sai tên dẫn đến trùng View; dự án chưa có tham số phân loại Browser. | Chỉ tạo 1 View 3D làm việc cá nhân có tiền tố tên cố định (vd: 3D_Working_HaiAnh). | **Ngày 2** *(Đã chọn làm công cụ đầu tiên)* |
| **3** | **Rà soát đấu nối hở và cảnh báo sai hệ thống** | **CƠ BẢN** | **KHẢ THI**<br>ConnectorSet, Connector.IsConnected, Selection | Báo nhầm các đầu bịt tự nhiên (miệng xả, van hở); quét toàn bộ mô hình lớn bị chậm. | Chỉ quét kiểm tra đầu hở cho Ống nước hoặc Ống gió trong View hiện tại, chọn đối tượng lỗi. | **Ngày 2** *(Bước chỉ-đọc an toàn)* |
| **6** | **Tạo bảng thống kê bóc tách khối lượng** | **CƠ BẢN** | **KHẢ THI**<br>ViewSchedule.CreateSchedule, ScheduleDefinition | Mỗi dự án có quy chuẩn cột và bộ lọc (Filter) khác nhau; khó chuẩn hóa chung. | Tạo tự động 1 bảng Schedule đếm số lượng đầu phun Sprinkler theo từng tầng. | **Ngày 2** hoặc **Ngày 3** |
| **1** | **Đổi tham số Mark cho đầu phun Sprinkler** | **TRUNG BÌNH** | **KHẢ THI**<br>FilteredElementCollector, FamilyInstance, Transaction | Trùng số với thiết bị khác làm Revit hiện cảnh báo vàng; gán nhầm ngoài vùng mong muốn. | Chỉ quét Sprinkler trong View hiện hành, đánh số thứ tự từ SP-001 đến hết. | **Ngày 3** *(Bài thực hành ghi tham số chuẩn)* |
| **5** | **Ghi chú tag hàng loạt trên mặt bằng** | **TRUNG BÌNH** | **KHẢ THI**<br>IndependentTag.Create, Reference | Tag bị đè nét lên nhau; dự án chưa nạp (load) sẵn Family Tag tương ứng. | Chỉ tag tự động kích thước cho các đoạn ống gió thẳng tại vị trí trung điểm ống. | **Ngày 3** *(Đã chọn làm công cụ thứ hai)* |
| **9** | **Đặt đèn theo vị trí bản vẽ CAD** | **PHỨC TẠP** | **KHẢ THI CÓ ĐIỀU KIỆN**<br>ImportInstance, GeometryInstance, FamilyInstance | CAD vẽ nét rời không lấy được tâm; Family đèn bám trần (Hosted) sẽ lỗi nếu không có host. | Chỉ nhận diện tâm Block CAD hình tròn và đặt loại đèn Non-hosted ở cao độ cố định. | **Ngày 4** hoặc **Để dành** |
| **4** | **Căn chỉnh nối ống nhánh vào ống chính** | **PHỨC TẠP** | **KHẢ THI CÓ ĐIỀU KIỆN**<br>LocationCurve, ElementTransformUtils | Lệch cao độ sinh ra góc nối kỳ dị; lỗi xung đột với Routing Preferences của hệ thống. | Chỉ dịch chuyển cao độ tim (Centerline) của ống nhánh bằng cao độ ống chính, chưa nối. | **Để dành** |
| **7** | **Kết nối các đoạn ống nhánh vào ống chính** | **PHỨC TẠP** | **KHẢ THI CÓ ĐIỀU KIỆN**<br>NewTeeFitting, NewTakeoffFitting | Khoảng cách đoạn nối quá ngắn không đủ đặt phụ kiện; không khớp đường kính phụ kiện. | Chỉ xử lý trường hợp 2 ống vuông góc 90° và cùng cao độ tim hoàn toàn. | **Để dành** |
| **10** | **Đặt giá đỡ cho hệ thống cơ điện** | **PHỨC TẠP** | **KHẢ THI CÓ ĐIỀU KIỆN**<br>ReferenceIntersector, FamilyInstance | Dầm nghiêng, sàn giật cấp khiến thuật toán bắn tia tìm đáy kết cấu bị tính sai chiều dài ti treo. | Đặt giá đỡ đơn theo khoảng cách đều 2m trên ống thẳng ở cùng 1 cao độ sàn (chưa bắt dầm). | **Để dành** |
| **8** | **Dựng hình hệ thống cơ điện từ CAD** | **PHỨC TẠP** | **KHÓ**<br>GeometryElement, PolyLine, Duct.Create | CAD 2D nét đứt, hở tim; không có cao độ 3D; thuật toán nối mạng phức tạp dễ crash mô hình. | Chỉ đọc các nét Line trên 1 layer CAD duy nhất để vẽ tim ống thẳng với cao độ cố định. | **Để dành** |

## Định hướng triển khai
- **Ngày 2 (Hiện tại):** Thực hiện Ý tưởng số 2 (Tạo view làm việc trên Project Browser).
- **Ngày 3 (Hiện tại):** Thực hiện Ý tưởng số 5 (Ghi chú tag hàng loạt trên View).
- **Ngày 4:** Nâng cấp tính năng hoặc thực hiện Ý tưởng số 5 / số 6.
- **Giai đoạn sau:** Nghiên cứu các bài toán kết nối hình học và CAD.
