DIENGAME - CLIENT / LOBBY V2 TEST 5 DAO

1) Dong Unity truoc khi cai.
2) SAO LUU project Unity hien tai.
3) Giai nen ZIP vao THU MUC GOC PROJECT (noi co Assets, ProjectSettings).
4) Chap nhan ghi de file. Khong keo thu muc Assets vao ben trong Assets!
5) Mo Unity; mo scene Assets/_Game/Scenes/Lobby.unity; bam Play.

Khong can chay Installer hoac Rebuild Lobby Scene: LobbyUI tu ve lai o Awake.

Da sua:
 - 5 dao duoc mo de test, khong hien KHOA/CHUA MO
 - Sprite dao 2-5 1024x1024 cap nhat qua Resources (client art moi)
 - Vien chon dao theo hinh dang dao, khong co hinh chu nhat mo
 - Them ten dao + thong tin man + anh preview + nut VAO AI
 - Giu nguyen navigation, 3 class va map gameplay da cai truoc do

Neu van thay lobby cu: check Unity Console compile errors, xac nhan file
Assets/_Game/Scripts/Client/LobbyUI.cs co dong LoadLatestWorldArt();
Va scene Lobby phai co component DienGame.Client.LobbyUI.

NOTE: chua thu Unity Play Mode trong phien nay.
