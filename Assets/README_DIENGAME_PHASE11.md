# DIENGAME — GIAI ĐOẠN 11: 5 ĐẢO, QUÁI & BOSS (QUY TÀNG)

## Bản nền
Kế thừa chính xác `DienGame_Phase10_FINAL_FULL.zip`. Không thay art, map, quái, animation hay nhân vật cũ. Không động đến PlayerPrefs.

## Điểm đã tích hợp

1. Phân bố 5 hệ quái (`Grunt`, `Runner`, `Tank`, `Ranged`, `Swarm`) khác nhau theo từng đảo 2–5; Đảo 1 giữ bản gốc.
2. Nhịp spawn đảo 3–5 được điều chỉnh rất nhẹ (nhanh hơn tối đa 4%, thêm tối đa 1 quái/đợt); giới hạn quái sống vẫn giữ `GameBalance.MaxAliveEnemies`.
3. Không tạo quái trong vật cản hoặc sát biên. Khi không tìm được điểm hợp lệ, bỏ lượt sinh thay vì ép sinh vào đá/tường. Boss tìm chỗ xa người chơi và có không gian để di chuyển.
4. Bản đồ đảo 2–5 chừa đường ngang + dọc liên tục giúp giảm khả năng chặn lối di chuyển trong map hình chữ nhật. Collision của các chướng ngại vẫn giữ nguyên.
5. Có sự kiện môi trường đảo 2–5 trong giai đoạn sinh tồn: vòng cảnh báo >= 1,45 giây, có thể né, không xuyên địa hình, sát thương 5,5–6,5% Max HP, mỗi 30–34 giây, lần đầu sau 38 giây.
6. Boss đảo 2–5: HP tăng có giới hạn 1,07–1,28 lần; đòn riêng tối đa mỗi 20–23 giây, có cảnh báo 1,25–1,40 giây, không trùng sát thương khi Boss đang thi triển chiêu cũ. Đảo 1 giữ Boss gốc.
7. Giữ nguyên toàn bộ 3 pha và kỹ năng `BossCombatController` hiện tại; đòn mới chỉ bổ trợ để các đảo có nhịp đấu riêng.
8. Thêm công cụ editor kiểm tra tài nguyên + nút đi thẳng tới Boss khi đang Play Mode.

## Định hướng truyện Quy Tàng
Vẫn dùng tên đảo, vũ khí, cơ–sinh và trường năng lượng đã có. Những tên Boss mới ở đây là **nhãn gameplay đề xuất**, không tự nhận là nhân vật/canon của truyện.

## QA trong Unity (bắt buộc)

**Cài:** sao lưu project; giải nén PATCH vào đúng thư mục gốc Unity (`Assets` nằm cạnh `Packages`/`ProjectSettings`). Nếu dùng FULL, nhập thư mục `Assets` vào project đã có Packages/ProjectSettings tương thích.

**Kiểm tra nội dung:** `DienGame > Phase11 > 01 - Kiem tra 5 dao + quai + Boss`.

**Kiểm tra đòn Boss nhanh:** vào trận ở Đảo 2–5, sau khi Play chọn `DienGame > Phase11 > 02 - Chuyen den Boss ngay (Play Mode)`. Chờ ~10 giây để thấy đòn mới; kiểm tra vùng cảnh báo và khả năng né tránh.

**Test thực tế:**
- Chạy lần lượt 5 đảo với 4 class, đặc biệt Đấu sĩ ở Đảo 5.
- Đảo 1 phải giữ hình ảnh, tần suất quái, Boss như trước.
- Đảo 2: nhiều đàn quái nhỏ; Đảo 3: quái tầm xa/giáp; Đảo 4: nhanh + giáp; Đảo 5: đội hình tầm xa.
- Đứng gần đá/cây/tường chờ đợt spawn; quái không xuất hiện đè vật cản; không bị vây kín hoàn toàn.
- Vòng cảnh báo phải vẽ trong thế giới, không che joystick/HUD. Thử đứng trong/vòng ngoài; chỉ bị đánh khi còn ở trong.
- Boss vẫn có ba pha chém/nổ/xung kích nguyên bản; đòn mới có thời gian cảnh báo, không spam liên tục.
- Vào menu Pause, tiếp tục, thắng/thua; không còn đòn môi trường sau khi kết thúc hoặc khi Boss xuất hiện.
- Build Android thực, thử 30/60 fps; kiểm tra cấp phát/bộ nhớ, vị trí spawn, pause, back-to-lobby.

## Giới hạn cần ghi nhận
- Chưa biên dịch hoặc chạy Unity Editor/Play Mode/Android trong môi trường tạo file. Đây là bản tích hợp và kiểm tra tĩnh, KHÔNG phải QA đã thông qua trên thiết bị.
- Phase11 sử dụng hình quái/Boss hiện tại, thêm cảnh báo LineRenderer; chưa có bộ quái/Boss vẽ mới độc lập.
- Không thay thiết kế cốt truyện hoặc xác nhận danh tính Boss canon khi chưa có nội dung truyện.
