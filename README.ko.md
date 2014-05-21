# NppDB.MSSQL
NppDB에서 MS SQL Server를 담당하는 모듈

## MS SQL Server에 연결
  1. 새로운 MS SQL Server를 등록한 다면 '+'아이콘을 눌러 'MS SQL Server'를 선택하거나
       Database Connect Manager root 노드 중에서 기존에 생성한 MS SQL Server 노드를 더블 클릭한다.
       
  2. 연결 정보 입력
      Server : 서버주소, 필수 항목
      Login ID : DB 계정, 필수 항목
      Password : 계정 암호, 필수 항목
      Initial Catalog : 최초 접속할 Database 이름, 옵션 항목
      Connection Timeout : 서버 접속 대기 시간으로 초 단위로 설정, 0초는 무제한 대기을 의미한다.
