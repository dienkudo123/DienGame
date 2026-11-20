DIENGAME / GIAI ĐOẠN 09 - CHIP KỸ NĂNG THÚ CƠ-SINH (QUY TÀNG)

BẢN GỐC: DienGame_Phase08_FINAL_FULL.zip. Chỉ bổ sung/sửa code liên kết.
Không xóa hay thay sprite, animation, map, prefab, chỉ số và tài nguyên lưu của Phase 08.
Không ghi đè Chimera cũ.

1) NỘI DUNG MỚI
- Tab THÚ / GIÁP / LÕI / CHIP trong TRANG BỊ khi chọn PHÁP SƯ.
- 5 thú cơ-sinh có chip độc lập: Hồ Ly Băng, Hỏa Điểu, Độc Tích, Hạch Hư Không, Lang Tử Quang.
- Chimera gốc không dùng chip và giữ trọn kỹ năng/animation cũ.
- Thú cơ-sinh phải được mở khóa, nâng +3 và trang bị mới lắp chip.
- Lựa chọn chip được lưu riêng cho từng thú trong PlayerPrefs; áp dụng khi vào trận.
- Tháo chip miễn phí. Không thêm summon thứ hai, không tăng thời gian khống chế.

2) CHIP VÀ ĐÁNH ĐỔI CÂN BẰNG (1 CHIP / THÚ)
- XUNG SĂN MỒI: mỗi 6 đòn thường, tối thiểu 8s, đánh thêm 22% sát thương đơn mục tiêu;
  giảm 4% sát thương thường trong suốt thời gian lắp.
- QUÉT XUNG: mỗi 7 đòn thường, tối thiểu 8s, đánh lan tối đa 2 kẻ địch trong 1.6 đơn vị;
  mỗi mục tiêu phụ nhận 16% sát thương; giảm 5% sát thương thường.
  Xung không xuyên chướng ngại MapObstacle chặn đạn.
- CỘNG SINH: mỗi 8 đòn thường, tối thiểu 12s, hồi 1.5% MaxHP chủ nếu thiếu máu;
  giảm 6% sát thương thường. Không hồi nếu chủ đã đầy máu/đã chết.
- Đòn phụ không đếm vào combo nội tại của pháp sư; giữ nguyên procs nguyên tố riêng từng thú.
- Các con số trên là cấu hình cân bằng thử nghiệm, cần kiểm thử DPS/boss thực tế.

3) CÀI ĐẶT
- Sao lưu project Unity trước khi cài.
- Nếu đang dùng Phase 08 FINAL: mở DienGame_Phase09_QuyTang_PATCH.zip,
  chép thư mục Assets/ vào thư mục gốc Unity có Assets/, cho phép ghi đè 2 file C#.
- Nếu cần đầy đủ Assets: dùng DienGame_Phase09_QuyTang_FULL.zip,
  KHÔNG xóa ProjectSettings/ hoặc Packages/ hiện có.
- Mở Unity, chờ import và compile. Không cần thao tác thiết lập Inspector.

4) KIỂM THỬ TRONG UNITY (BẮT BUỘC)
- Menu: DienGame > Equipment > 09 - Kiem tra thu co-sinh va chip.
- Lobby > Pháp Sư > Trang Bị > THÚ: mở, chọn, nâng 1 thú lên +3.
- Tab CHIP: thử lắp mỗi loại, thoát/reopen để xác nhận lưu, tháo chip.
- Thử Chimera gốc: tab CHIP báo không hỗ trợ, animation/skill y nguyên.
- Trong trận: đánh 6, 7, 8 đòn và kiểm tra cooldown 8s/12s; tránh bug kích hoạt nhiều lần.
- Test 1 địch, 3 địch, Boss, quái sau tường, trạng thái HP đầy/thấp.
- Kiểm tra joystick thứ hai, map/collision, nâng +10, các class khác, tỉ lệ màn hình.

GIỚI HẠN KIỂM CHỨNG
Đã kiểm tra cấu trúc mã nguồn, dữ liệu và file ZIP bằng Python.
Môi trường đóng gói KHÔNG có Unity Editor nên chưa test biên dịch/Play Mode/Android.
Các hoạt ảnh sử dụng atlas đã có từ Phase 03/08, chưa tạo sprite vẽ tay mới.
Đây là hệ thống local/offline PlayerPrefs, chưa có backend xác thực đồ cho multiplayer.
TÊN/MÔ TẢ chip là phiên bản thiết kế gameplay lấy cảm hứng Quy Tàng, không tự nhận là canon.
