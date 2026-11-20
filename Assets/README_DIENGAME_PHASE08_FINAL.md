# DienGame — GIAI ĐOẠN 08 HOÀN THIỆN: NGOẠI HÌNH TIẾN HÓA & CÂN BẰNG

NỀN BẮT BUỘC: DienGame Phase08 QuyTang (đã có UI v7, trang bị, lõi đồng bộ).
Không sử dụng bản Phase02 đã từng thay sprite Chimera; bản này không hề ghi đè các sprite gốc.

## Đã làm thật trong Unity

- 19 định nghĩa trang bị giữ nguyên ID/cost/behavior/save: Xạ thủ 5, Sát thủ 4, Pháp sư 6, Đấu sĩ 4 (Đấu sĩ chưa playable).
- Thêm 57 icon tiến hóa (19 × 3) ở `Resources/Equipment/Evolution/Icons/`.
- Thêm 15 sprite đạn tiến hóa (5 × 3) ở `Resources/Equipment/Evolution/Projectiles/`, giữ nguyên tỉ lệ 240px/260 PPU của đạn cũ.
- Thêm 3 khung hiệu ứng lõi riêng ở `Resources/Equipment/Evolution/Rings/`.
- +0..+2: nguyên bản; +3..+5: ỔN ÁP; +6..+9: CỘNG HƯỞNG; +10: ĐỒNG BỘ.
- UI tab TRANG BỊ hiển thị phiên bản trang bị theo cấp. Tab ĐỒNG BỘ LÕI xem trước hình các mốc +3/+6/+10.
- Đạn thường của Xạ thủ (kể cả đạn động năng mặc định khi nâng cấp) tự đổi sprite tier trong trận. Đạn kỹ năng chuyên biệt vẫn giữ asset và gameplay kỹ năng riêng.
- Đòn chém thường của Sát thủ hiển thị icon tiến hóa và vòng hiệu ứng ngắn, không đổi hitbox/tốc độ/sát thương.
- Chimera gốc và năm thú cơ-sinh giữ nguyên sprite sheet walk/attack/AI; chỉ bổ sung vòng lõi nhẹ phía sau thú theo cấp khế ước, đặt trên Visual child chứ không gắn với Rigidbody/collider.
- Các chế độ cân bằng, xung lực, nhịp nhanh ở +4 vẫn sử dụng công thức Phase08: 1.00/1.00, 1.10/1.12, 0.90/0.90. Không tăng DOT, CC, proc chance hoặc phạm vi.
- Không sửa map, skill tree, player classes hay save. Không tự cho phép Đấu sĩ vào combat khi class chưa tồn tại.

## Cài đặt

1. Sao lưu project Unity.
2. Nếu project đã là Phase08 QuyTang: giải nén `DienGame_Phase08_FINAL_PATCH.zip` tại thư mục có `Assets`, ghi đè chỉ 4 file `.cs` cần nối tính năng; các tệp còn lại là file MỚI.
3. Nếu project chưa cài Phase08 QuyTang: dùng `DienGame_Phase08_FINAL_FULL.zip` giải nén vào project (gói chỉ bao gồm Assets; giữ Packages và ProjectSettings của bạn). Không trộn bản cũ trùng tính năng mà không sao lưu.
4. Mở Unity và để import hoàn tất. Không cần kéo asset vào Inspector. Thử mở Lobby -> Trang Bị -> Vũ Khí/Thú -> Đồng Bộ Lõi.
5. Editor > DienGame > Equipment > 08 FINAL - Kiem tra hinh tien hoa để xác nhận Resources.Load<Sprite> thành công.

## Kịch bản kiểm thử trong Play Mode (phải chạy trên máy)

1. Một món +0 hiển thị icon gốc, nâng +3 đổi icon/màu viền, +6 và +10 khác biệt rõ; vào trận cùng cấp tương ứng.
2. Xạ thủ: bắn đạn thường và 4 loại đạn đặc biệt ở +3/+6/+10; xem có giữ cỡ đạn, không nhảy scale và không để vết ảnh cũ khi projectile bị pool/reuse.
3. Sát thủ: chém thường cả 4 vũ khí; xem vòng lõi nhỏ và fade sạch sau 0.16s; collider và số lượng quái trúng giữ nguyên.
4. Pháp sư: Chimera và 5 thú mới; vòng lõi theo cấp hiện phía sau thú, không khiến chuyển động/joystick/hitbox giật, không tự phát sinh skill.
5. So sánh DPS 10–30 giây cả 3 chế độ (+4 trở lên), mục tiêu ±3,5% tương quan lý thuyết trước bonus trạng thái; hiệu ứng slow/poison/burn không tăng thời lượng từ ngoại hình.
6. Kiểm tra nút nâng, mở khóa, lưu/chọn trang bị và chuyển cảnh trên 720×1280, 1080×1920, 1080×2400.

## Kiểm chứng

Đã kiểm tra bằng code: 19 định nghĩa, tên/đường dẫn và 75 asset dạng PNG đúng cấp, 260 PPU cho đạn, file Sprite meta không trùng GUID, SHA-256 không đổi của các file gốc (ngoại trừ 4 script tích hợp), không xóa file cũ. ZIP test đầy đủ.
**Chưa chạy được Unity Editor để compile và Play Mode; chưa xác nhận hoạt động Android/WebGL hay FPS thực.** Vì vậy trạng thái cuối là `đã tích hợp và kiểm tra tĩnh`, cần QA trong Unity trước khi chốt phát hành.

## Hệ Quy Tàng

Ngoại hình lấy cảm hứng từ công nghệ, trường X và giao thoa cơ-sinh. Tên món, cường hóa và lõi là sáng tạo gameplay, KHÔNG khẳng định thuộc canon truyện. Giữ nguyên tuyến nhân vật và GDD hiện có.
