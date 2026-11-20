# DienGame — Giai đoạn 10 FINAL (Unity Assets)

**Nền:** `DienGame_Phase10_QuyTang_FULL.zip` / `DienGame_Phase10_QuyTang_PATCH.zip` từ giai đoạn trước. Bản này **không** dựa vào ZIP Phase 2 cũ từng ghi đè Chimera.

## Đã bổ sung / sửa

1. **Atlas thân Đấu sĩ chuyên biệt** ở `Assets/_Game/Resources/Warrior/`:
   - `WarriorRun8x4.png`: 8 hướng × 4 frame chạy;
   - `WarriorSlash8x4.png`: 8 hướng × 4 frame chém;
   - `WarriorArt.cs` nạp/cắt đúng lưới bằng code (không thiết lập Sprite Editor bằng tay).
   - Hai atlas là **biến thể thiết giáp được xử lý lại từ pose/sprite Sát thủ có sẵn**, không phải bộ artwork Đấu sĩ vẽ hoàn toàn mới từ đầu. Ảnh Sát thủ gốc không bị đổi. `WarriorVisual` ưu tiên atlas Đấu sĩ, chỉ dùng atlas Sát thủ nếu thiếu hình.
2. **Tính năng kiếm và chiến đấu:** sửa hệ số chu kỳ đòn của Đồng bộ lõi bị nhân đôi (đã được áp vào `PlayerCharacter` khi vào trận); tìm địch gần có đường đánh không bị chặn; chọn mục tiêu theo thứ tự gần nhất để giữ giới hạn 2/4 mục tiêu. Combo ba nhịp, 7 kỹ năng, 4 kiếm, kỹ năng và hiệu ứng đang có tiếp tục sử dụng.
3. **Chọn kỹ năng:** không còn tự nhảy lên cấp 10 khi vừa vào scene Đấu sĩ trong Unity Editor. Vẫn nhận skill theo EXP thực tế.
4. **Bảo vệ bản build:** thêm `DienGamePhase10FinalValidator` tự kiểm tra scene Warrior thực sự có WarriorMeleeAttack, WarriorVisual, skill controller và không còn AssassinMeleeAttack; kiểm tra atlas 4×8, texture readable và 19 định nghĩa trang bị. Build bị chặn có thông báo nếu scene Warrior chưa được cài (tránh nhầm class khi phát hành).
5. **Scene installer:** nếu scene Warrior bị chép đè thành bản Assassin mặc dù EditorPrefs nói đã cài, installer phát hiện lại theo mã component trong YAML và tự cài lại khi Editor an toàn.
6. Không sửa dữ liệu PlayerPrefs người chơi. Chế độ **19/19 trang bị mở khóa để test** từ Phase09 vẫn được giữ nguyên, không tự biến thành tiến trình người chơi thật.

## Cài đặt

- **Đang dùng Phase10:** backup project; giải nén `DienGame_Phase10_FINAL_PATCH.zip` vào **thư mục gốc Unity project** (nơi có `Assets/`), cho phép ghi đè các file C# được liệt kê.
- **Chưa có Phase10:** dùng `DienGame_Phase10_FINAL_FULL.zip` như **bộ Assets** đưa vào Unity project tương thích; file ZIP không bao gồm Packages/ProjectSettings nên **không** phải Unity project chạy độc lập.
- Chờ Unity Editor import/compile. Nếu đang có scene thay đổi chưa lưu, lưu scene trước.
- Chạy `DienGame > Phase10 > 01 - Cai dat Dau si (scene + build settings)` nếu scene chưa được cài tự động.
- Chạy `DienGame > Phase10 > FINAL - Kiem tra scene + animation + trang bi` để xác minh. Bộ kiểm tra sẽ **dừng build** nếu scene Đấu sĩ chưa đúng.

## Checklist Play Mode / Android (bắt buộc kiểm chứng trên máy của m)

- [ ] Vào Lobby -> Đấu sĩ -> vào ải, hiển thị **thân giáp xanh thép/vàng**, sword overlay; không hiển thị sprite nguyên bản Sát thủ.
- [ ] Joystick 360°; 8 hướng, frame chạy/chém; không bị trôi sprite khỏi vị trí/collider.
- [ ] Đấu sĩ dùng 4 kiếm khác nhau, thay hình kiếm và hiệu ứng/nhịp đánh tương ứng.
- [ ] Kiếm hợp kim: chém 150°, tầm 1.85; tối đa 2 quái; combo nhịp 3.
- [ ] Đại kiếm: 225°, tầm 2.45; tối đa 4 quái; đánh chậm hơn.
- [ ] Kiếm lửa/kiếm băng: trạng thái có thời gian hữu hạn, không cộng dồn vô hạn.
- [ ] Đồng bộ lõi +4 nhịp nhanh / xung lực: đánh đổi damage/chu kỳ theo đúng UI; không nhân đôi chu kỳ.
- [ ] Quái phía trước bị tường che: đổi sang quái khác trong tầm đánh, không xuyên tường.
- [ ] 7 kỹ năng nâng bằng EXP bình thường; không tự xuất liên tiếp nhiều popup ở đầu trận.
- [ ] Mở khóa/trang bị 19 vật phẩm trong chế độ test; tắt QA vẫn còn save thật nguyên vẹn.
- [ ] Xạ thủ, Sát thủ, Pháp sư/Chimera + 5 thú cơ sinh vẫn chơi được như bản trước.
- [ ] Scene tạm dừng, quay về sảnh, chơi lại; kiểm tra UI trên 360×800, 393×852, 1080×1920; 30/60 FPS trên Android.

## Giới hạn xác nhận

Bản này được **kiểm tra tĩnh**, danh sách thay đổi, hình atlas và ZIP. **Chưa chạy Unity Editor Play Mode, chưa build Android**, nên không khẳng định đã hết mọi lỗi gameplay/hiển thị. Atlas Đấu sĩ mới là đồ họa **chuyển thể từ pose đã có**, cần thuê/vẽ lại sprite 8 hướng hoàn toàn mới nếu muốn bản nghệ thuật cuối cùng không còn nguồn từ Sát thủ.

**Định hướng Quy Tàng:** thiết giáp, giao thoa trường X và kiếm cơ giới là diễn giải gameplay; không xác nhận nội dung nào là canon truyện.
