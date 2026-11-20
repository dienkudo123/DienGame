# DienGame – Giai đoạn 07 / UI v7: Layout & usability

## Nền của bản cập nhật
Bản này được xây trên **DienGame UI v6 FULL**. Các file game, island art, trang bị, animation, kỹ năng, Chimera và dữ liệu cũ vẫn giữ nguyên. Không sử dụng patch Phase 2 thay ảnh Chimera.

## Những phần đã sửa
1. **Lobby/top bar:** bốn khung tài nguyên/profile dùng tỷ lệ theo chiều ngang, không còn đặt x = 840 hoặc kích cỡ cố định; thay emoji năng lượng bằng chữ `NL` để tránh ô vuông thiếu glyph.
2. **Nhãn năm đảo:** dùng tên ngắn ở map (ví dụ `ĐẢO 3 · CÁT VÀNG`), tên đầy đủ và mô tả vẫn nằm trong thẻ chọn đảo. Loại bỏ hàng sao riêng bên dưới mỗi đảo để tránh đè vào nút/ảnh; hàng sao của thẻ thông tin vẫn còn. Nới vùng chữ điểm danh.
3. **Thanh menu dưới:** icon và tên tab chiếm hai vùng chiều cao tách biệt, không chen vào nhau.
4. **Font tiếng Việt:** trước khi thử font hệ điều hành, dùng TMP font fallback dynamic đã có sẵn trong project. Các text mới hiển thị dấu chấm lửng khi không thể chứa toàn bộ nội dung thay vì tràn qua mép.
5. **Popup:** nội dung chọn nhân vật, trang bị, nhiệm vụ, cửa hàng và lên cấp gắn vào vùng an toàn để tránh tai thỏ/ thanh điều hướng. Nền che vẫn phủ toàn màn hình.
6. **Tạm dừng:** sửa mô tả ngắn, giữ ba nút bên trong khung; giấu artwork/cảm ứng của joystick khi menu hiển thị và khôi phục khi tiếp tục, không thay bộ điều khiển 360 độ.
7. **Kết thúc trận:** tiêu đề và hai nút là con của khung kết quả, neo bằng tỷ lệ thay vì vị trí pixel tuyệt đối.
8. **Joystick:** tự tạo ring dạng tròn nếu asset ring không load; tạo Image cho stick/handle nếu chưa có, tránh mặc định hình vuông xám.
9. **Joystick của thú:** nhãn trạng thái nằm trong chiều cao widget thay vì lòi ra phía trên.
10. **QA trong Editor:** kiểm tra chữ tràn/cắt, rect ngoài parent, sprite của joystick, button thiếu Graphic; có thể xuất ảnh Play Mode và file báo cáo lỗi.

## Cách cài
1. Backup Unity project của m trước.
2. Nếu đang chạy **UI v6**, giải nén `DienGame_UI_V7_FIX_PATCH.zip` vào thư mục gốc project (nơi có thư mục `Assets`), cho phép ghi đè đúng các file UI.
3. Nếu cần một bộ mới, `DienGame_UI_V7_FULL.zip` là bản v6 đã tích hợp v7. Chỉ có `Assets/`, không chứa các thư mục `Packages/` hay `ProjectSettings/` của project m. Bộ FULL không phân phối lại font TTF gốc đi kèm gói TextMesh Pro; hãy giữ gói TMP/Essential Resources đã có trong Unity project.
4. Cho Unity reimport/compile. Nếu Console có lỗi, giữ nguyên log và dừng Play cho đến khi sửa.

## Kiểm thử trong Unity (bắt buộc trước khi coi là hoàn thành)
- Trong Game view thử **720×1280**, **1080×1920**, **1080×2400**, **1440×3200**, thêm Safe Area/notch nếu có máy thật.
- Trong Lobby: top bar (class/HP/vàng/gem), xem đủ 5 đảo, bấm Điểm danh, bấm từng đảo, kiểm tra tên, sao và nút Vào ải. Mở chọn nhân vật cho cả ba class. Kiểm tra tên class và trạng thái chọn.
- Mở **Trang bị**, kéo danh sách nhiều loại, mở khóa/thử nâng cấp; kiểm tra kết quả sau khi vào trận. Mở **Kỹ năng**, **Nhiệm vụ**, **Cửa hàng**; bấm nút, xác nhận giá/nhận thưởng không đổi.
- Vào trận: thử joystick di chuyển 360°, Pháp sư dùng joystick thú, Pause/Resume/Restart/Return to Lobby, lên cấp với mô tả kỹ năng dài, thắng/thua và thanh máu Boss. Chữ không được nằm ngoài khung và icon không biến thành hình vuông xám.
- Khi đang Play, mở mỗi popup và chọn **DienGame > UI > Check Visible Layout (Play Mode)**. Tool xuất `DienGame_UI_V7_QA_Report.txt` tại gốc project. Nếu cần gửi ảnh lỗi, dùng **DienGame > UI > Capture Game View (Play Mode)**.

## Những gì đã kiểm tra bên ngoài Unity
- Đối chiếu hash từng file giữa v6 và v7: **không xóa file nào**, các file game/ảnh/map/trang bị/scene/prefab không bị thay.
- Kiểm tra cấu trúc dấu ngoặc source C#, các đường dẫn resource UI, anchor của layout và hình học nhãn đảo trên 7 tỉ lệ mobile.
- Kiểm tra toàn vẹn file ZIP.

## Giới hạn
Môi trường dựng patch không chạy được Unity Editor; do vậy đây là bản **đã sửa mã nguồn nhưng chưa được chứng nhận Play Mode**. Một số lỗi còn phụ thuộc font/bản build/device và phải chạy menu QA trong Unity để xác nhận hết tràn chữ. Nếu vẫn có lỗi, gửi Console + ảnh Game View + `DienGame_UI_V7_QA_Report.txt`.
