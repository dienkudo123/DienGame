# DienGame — Giai đoạn 08: Trang bị · Lõi đồng bộ Quy Tàng

**Nền tích hợp:** DienGame UI v7 FULL. Đây là thiết kế gameplay lấy *vibe* từ Quy Tàng, không tuyên bố bổ sung canon của truyện. Các điểm tham chiếu: Trái Đất có trường Einstein (X) dày; Sao Hỏa X mỏng và thiên về khoa học/cơ khí; bối cảnh hậu tận thế, công nghệ và hiện tượng huyền bí cùng tồn tại. Chưa gán bất kỳ món vũ khí cụ thể nào là di vật hay sản phẩm canon thuộc một phe/nhân vật.

## Đã thêm và tích hợp

1. **Bộ tên/miêu tả giao thoa khoa học–trường X** cho đạn Xạ thủ, vũ khí Sát thủ, Chimera và kiếm Đấu sĩ (Đấu sĩ vẫn chưa có class playable). ID, chi phí, quyền sở hữu, kỹ năng, hình ảnh và save cũ không đổi.
2. **Mốc hiển thị tiến hóa** +3 (ổn áp), +6 (cộng hưởng), +10 (đồng bộ). Đây là tiến hóa màu hiển thị mức độ nhẹ, không cộng chỉ số ẩn, không đổi sprite gốc, không cho hiệu ứng khống chế mạnh hơn.
3. **Mở điều chỉnh lõi tại +4:** tab `ĐỒNG BỘ LÕI` trong `TRANG BỊ` của Lobby. Chọn một trong ba cấu hình lưu *riêng từng món* qua PlayerPrefs.
   - Cân bằng: sát thương thường ×1.00, nhịp đánh ×1.00, DPS tương đối 1.000.
   - Xung lực: sát thương thường ×1.10, *thời gian giữa các đòn* ×1.12, DPS tương đối 0.982.
   - Nhịp nhanh: sát thương thường ×0.90, *thời gian giữa các đòn* ×0.90, DPS tương đối 1.000.
   - Chỉ số +level toàn cục tiếp tục dùng hệ thống cũ (+3.5% damage per level). Không tăng proc rate, crowd control, AoE targets hay cấp skill; mode thay nhịp *normal attack* và sát thương *normal attack*.
4. **Áp dụng combat:** normal bullet của Xạ thủ, đòn chém thường Sát thủ, tia phép thường Pháp sư, và đòn đánh thường Chimera/thú cơ–sinh (kể cả tốc độ riêng thú). Kỹ năng chủ động/special giữ hệ số và hồi chiêu gốc. Vật phẩm cũ còn 100%.
5. **Hiệu ứng ngoại hình:** sprite của đạn/sát thủ/thú có sắc độ nhẹ theo +level, không làm đổi hitbox/sprite sheet. Không tạo asset fantasy mới.
6. **Menu editor tự kiểm:** `DienGame > Equipment > 08 - Kiểm tra lõi Quy Tàng`.

## Cài đặt
- **Nếu đang ở UI v7:** Sao lưu project, giải nén `DienGame_Phase08_QuyTang_PATCH.zip` tại thư mục gốc project chứa `Assets/`, cho phép ghi đè các script nêu trong patch.
- **Nếu muốn đầy đủ từ đầu:** `DienGame_Phase08_QuyTang_FULL.zip` chứa `Assets/` tích lũy tới v7 cộng Phase08; không có `ProjectSettings`/`Packages` của máy bạn. Giữ nguyên những file này trong project.
- Để Unity compile. Không import patch này trên v5/v6/Phase4 vì khác base.

## Checklist test trong Unity (chưa chạy được Editor tại đây)
1. Mở Lobby -> TRANG BỊ -> trang VŨ KHÍ/THÚ. Xác nhận tên mô tả có dấu và không tràn.
2. Dùng menu `DienGame > Equipment > 02 - Cộng 2000 vàng và 50 mảnh (PLAYTEST)` nếu cần. Cường hóa một món đến +4; mở ĐỒNG BỘ LÕI; thử CÂN BẰNG, XUNG LỰC, NHỊP NHANH. Kiểm tra nút khóa khi +0...+3, chọn xong thoát Lobby và vào lại vẫn giữ mode.
3. Vào trận bằng từng class đã playable, so số đòn / 10 giây và sát thương; phiên bản Nhịp nhanh đánh nhanh hơn nhưng sát thương/đòn yếu hơn.
4. Thử tất cả thú cơ–sinh và Chimera gốc, check sprite/animation/AI cũ. Thử skill chủ động khi thay mode để kiểm tra cooldown giữ nguyên.
5. Lặp ở +3, +6, +10; đánh boss, xác nhận slow/poison/burn không thay thời lượng theo cấp.
6. Test Play Mode trên 720x1280, 1080x1920 và 1080x2400. Sử dụng menu UI Layout QA v7; gửi Console + báo cáo nếu có lỗi.

## Kiểm chứng ngoài Unity
- Kiểm tra source C# thay đổi có delimiter cân bằng và các hook thuộc đúng class; kiểm tra hiệu ứng trực tiếp bằng phân tích công thức.
- Đối chiếu bộ v7: không sửa sprite/map/scene/prefab/pet atlas/skill tree, không xóa file; chỉ cập nhật các script liệt kê trong PATCH và thêm 2 script mới.
- **Chưa compile bằng Unity Editor/chưa test Play Mode hoặc Android.** Không coi là bản QA đạt cho đến khi chạy trong Unity của bạn.

## Phân biệt canon và sáng tạo gameplay
- Trường X, tương phản Trái Đất/Sao Hỏa là những điểm thế giới quan đã thảo luận.
- `Đạn Hàn X`, `Đồng bộ lõi`, `Ổn áp`, v.v. là **tên và cơ chế gameplay đề xuất**, không phải vật phẩm hay thuật ngữ canon đã được xác minh từ tác phẩm.
- Không thêm phe, nhân vật, địa điểm hay sự kiện cốt truyện mới.
