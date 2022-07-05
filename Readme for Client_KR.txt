[클라이언트 구축 및 실행 방법]

- 실행환경 
 1. Visual Studio 2022 Commnunity
    - .NET 4.7.2 프레임 설치
 2. 윈도우 디스플레이 설정 DPI 100%로 변경

 3. ALPR_Client 폴더 확인
   1) 클라이언트 키 파일 더블클릭하여 기본 설치 (Client.pfx, PW: qwe123..) 위치 : ALPR_Client\Key 폴더
   2) 웹서버 구축 및 실행 (서버쪽 가이드 통해 설치), 서버쪽 IP확인함
   3) OpenALPR.sln 솔루션 열기
   4) lgdemo_w (MFC프로젝트) 와 WindowsForms_Clien(C# 프로젝트) 프로젝트 존재확인
   5) 정상 빌드  확인 (안에 필요한 라이브러리와 참조연결을 해놓았기 때문에 폴더구성을 흩트리지않으면 정상빌드가 가능해야함)
   6) WindowsForms_Client를 시작프로젝트로 지정
   7) form1.cs파일 58라인의 serverURL을 2)에서 얻은 서버 IP주소로 변경 (로컬서버로 구성한 경우 그대로)
   8) 빌드 실행하여 정상적으로 윈도우 클라이언트 앱이 실행되나 확인 (ALPRClient.exe)

