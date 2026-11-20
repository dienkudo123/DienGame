DIENGAME — PHASE 09 FINAL: THÚ CƠ-SINH + TEST TOÀN BỘ TRANG BỊ
================================================================
NỀN: DienGame_Phase09_QuyTang_FULL.zip / Phase 09 đã có chip, 5 thú biomech, Chimera gốc.
Không xóa hoặc ghi đè sprite, animation, map, prefab, skills và dữ liệu thật.

1. CÁCH CÀI
- Sao lưu project Unity trước khi nhập.
- Đã có Phase 09: giải nén DienGame_Phase09_FINAL_TEST_PATCH.zip; chép Assets/ vào thư mục gốc project Unity (cạnh ProjectSettings/ và Packages/).
- Muốn toàn bộ Assets: dùng DienGame_Phase09_FINAL_TEST_FULL.zip, chỉ bổ sung thư mục Assets, giữ ProjectSettings/ và Packages/ của project hiện tại.
- Đợi Unity import. Nếu thấy lỗi trong Console, sửa compile trước khi test.

2. MỞ KHÓA 100% TRANG BỊ TEST (MẶC ĐỊNH BẬT TRONG EDITOR)
- Trong UNITY EDITOR hoặc DEVELOPMENT_BUILD: tất cả 19 định nghĩa trang bị được coi là mở khóa; không cần vàng, mảnh hoặc rương.
- 5 Xạ thủ | 4 Sát thủ | 6 Pháp sư (Chimera + 5 thú cơ-sinh) | 4 Đấu sĩ.
- Đấu sĩ hiện chỉ có DATA/ICON trang bị, chưa có màn chiến đấu riêng (giai đoạn 10).
- Mức cường hóa core mặc định trong TEST = +3 => lắp CHIP thú biomech ngay.
- Muốn kiểm tra các mốc tiến hóa, vào menu Unity:
    DienGame > Equipment > 09 FINAL TEST > 02 - Cap thu +0
    DienGame > Equipment > 09 FINAL TEST > 03 - Cap thu +3 (CHIP)
    DienGame > Equipment > 09 FINAL TEST > 04 - Cap thu +4 (LOI)
    DienGame > Equipment > 09 FINAL TEST > 05 - Cap thu +6
    DienGame > Equipment > 09 FINAL TEST > 06 - Cap thu +10
- Nâng cấp trong màn TRANG BỊ miễn phí trong TEST, không trừ vàng/tinh thể, cấp tối đa core +10, giáp/phù ấn +5.
- Trong Editor, chọn DienGame > Equipment > 09 FINAL TEST > 09 - Kiem tra asset, ID va unlock
  để kiểm tra 19 món, icon, 180 frame của 5 thú, asset thiếu và dữ liệu.

3. KHÔNG ĐỤNG SAVE THẬT
- Test trạng thái trang bị/chọn class item/chip/hiệu chỉnh lõi được lưu trong namespace diengame_phase09_qa_*.
- Không ghi đè unlock/equip/level core thật, chip thật, hiệu chỉnh lõi thật, vàng, mảnh, giáp/phù ấn thật.
- Tắt TEST: DienGame > Equipment > 09 FINAL TEST > 07 - Tat TEST, dung save that.
- Xóa chỉ các thay đổi TEST: > 08 - Xoa du lieu TEST (giu save that).
- BUILD THƯỜNG (non-development): luôn TẮT QA; trang bị lại theo dữ liệu save bình thường.
- TEST mặc định BẬT chỉ ở Unity Editor và Development Build; cần tắt thủ công trong Editor nếu muốn đo DPS theo tiến trình thật.

4. PHASE 09 FINAL: THÚ CƠ-SINH
- Chimera gốc không bị thay sprite/AI/chip.
- 5 thú mới dùng atlas 4 hướng: 4 frame đi + 5 frame đánh.
- Mỗi thú có phong cách di chuyển/đánh khác nhau (tốc độ, tầm, nhịp đánh) ở biên độ nhỏ, vẫn theo quy tắc leash của chủ và collision địa hình.
- 3 chip kỹ năng (Xung Săn Mồi / Quét Xung / Cộng Sinh) áp dụng lên đòn đánh thường.
- Quét Xung không mất proc khi chưa có mục tiêu hợp lệ; lan nguyên tố không qua MapObstacle chặn đạn.
- Không tự tăng khống chế/độc theo cấp; đòn phụ không kích hoạt lại combo.
- Các con số hiện là baseline cân bằng để test, chưa phải kết quả cân bằng từ dữ liệu người chơi.

5. CHECKLIST PLAY MODE (BẮT BUỘC LÀM TẠI MÁY CÓ UNITY)
[ ] Mở Lobby -> chọn Xạ thủ: 5/5 món mở; đổi từng loại đạn, vào trận xác nhận projectile/VFX.
[ ] Mở Lobby -> Sát thủ: 4/4 món mở; so tầm, góc quét và sát thương.
[ ] Mở Lobby -> Pháp sư: 6/6 món mở; Chimera giữ animation/kỹ năng gốc.
[ ] Chọn từng thú biomech, vào trận test 4 hướng di chuyển, animation 5 frame tấn công, leash, joystick 2.
[ ] Chip của từng thú: lắp/tháo/cất cấu hình; test 6/7/8 đòn, cooldown 8/8/12 giây.
[ ] Quét Xung không nổ nếu không có mục tiêu phụ; đánh sau tường không áp hiệu ứng xuyên tường.
[ ] Cộng Sinh không hồi khi đầy HP; bắt đầu hồi khi thiếu máu.
[ ] Test +0/+3/+4/+6/+10 (UI, ngoại hình/tiến hóa, chip và hiệu chỉnh lõi).
[ ] Khi QA ON, vàng/mảnh không thay đổi sau cường hóa.
[ ] Tắt QA, kiểm tra kho thật và số dư thật được giữ nguyên.
[ ] Test UI màn dọc, tỷ lệ 9:16/19.5:9, font tiếng Việt, FPS Android.
[ ] Test boss: hiệu ứng không khóa cứng boss, không xuyên map, không crash khi số lượng quái lớn.

6. GIỚI HẠN XÁC MINH
- Các file được kiểm tra tĩnh và đóng gói ZIP; chưa có Unity Editor trong môi trường đóng gói để chạy biên dịch, Play Mode, thiết bị Android hoặc đo DPS.
- Không tạo lại artwork của Chimera hoặc các thú; sử dụng toàn bộ atlas đang có từ Phase 09.
- Đây là test offline PlayerPrefs; bản phát hành/multiplayer cần xác thực trang bị bên server.
