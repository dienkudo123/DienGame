# DIENGAME – UI v6 – SỬA TRÀN KHUNG

## Cài đặt
- Sao lưu project trước khi áp bản vá.
- Nếu đang dùng DienGame UI v5: giải nén PATCH vào thư mục gốc Unity (thư mục chứa Assets).
- Chỉ các script UI đã chỉnh mới được ghi đè. Không xóa scene hoặc gameplay.
- Để Unity import/compile xong trước khi chạy Play.

## Đã sửa
1. Đảo trên bản đồ: nhãn chia làm hai dòng, font vừa khung, nới khung và đẩy đảo 1 khỏi panel thông tin.
2. Menu dưới: 5 nút co theo bề rộng, icon và caption giữ ở trong nút.
3. Card nhân vật: trạng thái ĐANG CHỌN không còn đè lên tên class.
4. Panel trang bị, kỹ năng, nhiệm vụ, cửa hàng: tinh chỉnh chữ dài, nút và vùng mô tả.
5. Popup lên cấp: giảm kích thước tiêu đề/mô tả, tăng khoảng hiển thị text.
6. Menu tạm dừng/chơi mới: thay bằng panel có khung, tiêu đề, mô tả và ba nút bố trí đúng form.
7. Gameplay Joystick: dùng vòng joystick PNG và thumb tròn được tạo bằng code, không còn hình vuông xám/tròn đỏ; vẫn giữ OnScreenStick.
8. TextMeshPro: tự co chữ từ kích thước chuẩn và không vẽ ký tự ra ngoài vùng chứa.

## Các bước QA cần chạy trong Unity
- Chọn Xạ thủ, Sát thủ, Pháp sư, mở tất cả 5 đảo ở độ phân giải 1080x1920, 720x1280, 1440x3200.
- Mở/cắt class panel, trang bị (kéo danh sách), nhiệm vụ, cửa hàng, kỹ năng. Chữ không nằm ngoài card/khung.
- Vào trận: chạm thử joystick, nút tạm dừng, tiếp tục, chơi lại, về sảnh.
- Lên cấp nhiều lần với mô tả kỹ năng dài (cả 3 class).
- Kiểm tra notch/safe area trên thiết bị thực; xác nhận ảnh HUD HP/EXP hiện đúng.

## Giới hạn kiểm chứng
Không có Unity Editor trong môi trường dựng patch, nên đây là bản sửa mã nguồn và kiểm tra tĩnh, chưa có test Play Mode thực tế. Không thay sprite/map/khế ước/thú/logic combat.

## Kiểm tra tự động
Trong Play Mode, có thêm menu DienGame > UI > Check Visible Layout (Play Mode).
Bật từng popup rồi chạy menu để log các chữ có khả năng quá rộng/cao hoặc joystick thiếu sprite.
Hệ thống audit chỉ đọc UI, không sửa scene.
