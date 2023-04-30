# com2usServerCamp 연규준
컴투스 서버캠프 과제 - 던전파밍

## 과제 진행 방식
main branch를 기점으로, 새로운 기능 추가시
1. 이슈를 판다
2. 이슈에 대응하는 branch를 main으로 부터 만든다
3. 기능 구현 후 main에 merge한다

위와 같은 방식으로 기능 구현중 입니다.

## 과제 진행도
- [ ] : 유니티 스타트

# 필수 구현 조건
- [ ] Web API서버는 스케일아웃이 가능해야 한다.
- [ ] 로드밸런스를 사용하고 있다고 가정한다.
- [ ] 인증과 관련해서 Redis를 꼭 사용해야 한다. JWT 사용 불가
- [ ] 인증을 한 후(즉 로그인 후)의 모든 요청에 대해서 아래를 꼭 확인한다
- [ ] 인증 받은 유저인지 확인
- [ ] 클라이언트 앱 버전 확인
- [ ] 클라이언트 마스터 데이터(기획 데이터) 버전 확인
- [ ] DB는 샤딩을 하지 않는다.
- [ ] 마스터 데이터는 서버 실행 후에는 변경되지 않는다.
- [ ] DB는 MySQL, Redis만 사용한다.
- [ ] Redis는 in-memory로만 사용한다

# 구현할 기능
## 계정 생성
- [ ] 아이디, 패스워드만 저장한다
- [ ] 아이디 중복 체크는 필수
- [ ] 패스워드 보안도 중요하다
- [ ] 캐릭터 생성 기능은 없다.
- [ ] 기본 데이터를 생성한다.
- [ ] 기본 게임 데이터, 기본 아이템 데이터
## 로그인
- [ ] 자신의 게임 데이터를 로딩한다.
- [ ] 기본 게임 데이터, 아이템 데이터
- [ ] 앱 버전, 마스터데이터 버전을 확인한다.
- [ ] 로그인 성공 시 서버에 등록된 공지를 클라이언트에게 전송해야 한다.
## 공지 등록 및 전송
- [ ] 공지는 Redis에 저장 되어 있다고 가정한다.
- [ ] 미리 데이터를 넣어 놓아야 한다.
- [ ] 수동으로 넣어도 괜찮다.
- [ ] 서버 실행 중 공지 변경은 없다
- [ ] 공지의 최대 크기는 1K 이다.
## 우편함
- [ ] 우편 저장 개수는 무한대이다.
- [ ] 클라이언트에서는 1개 페이지마다 20개씩 표시된다.
- [ ] 클라이언트에서 우편함을 처음 열었다는 판단은 서버에서 페이지 번호 1로 판단한다.
- [ ] 이후 클라이언트에서는 열어본 페이지에 대해서는 API 호출을 하지 않는다.
- [ ] 새롭게 우편함을 열기 전까지는 클라이언트는 우편 데이터는 캐싱한다.
- [ ] 우편은 유효 기간이 있다. 유효 기간이 지나면 삭제된다. 
- [ ] 삭제는 외부에서 정기적으로 한다고 가정한다. 즉 삭제 기능을 구현하지 않아도 된다.
## 출석부
- [ ] 30일이 넘으면 다시 1일부터 시작한다.
- [ ] 연속으로 출석을 하지 않으면 다시 1일부터 시작한다.
- [ ] 보상은 우편함을 통해서 준다. (지급 내용은 마스터데이터의 출석부보상 시트 참고)
## 인앱 결제 아이템 지급
- [ ] 클라이언트가 구글 혹은 애플 스토어에서 인앱을 샀다고 가정한다.
- [ ] 클라이언트는 서버에 지급 요청을 한다. 구매한 인앱 영수증을 보내야 한다. 영승증 정보는 임의로 만든다.
- [ ] 서버는 영수증 검증은 실제 하지 않는다. 클라이언트에서 올바른 영수증을 보내었다고 가정한다.
- [ ] 단 중복 요청은 꼭 검증해야 한다.
- [ ] 문제 없으면 아이템을 우편함으로 지급한다.
## 강화
- [ ] 강화 최대 횟수 제한은 있어야 한다.
- [ ] 무기, 방어구만 강화 가능하다.
- [ ] 아이템 정보 중 최대 강화 단계 가 0인 아이템은 강화 불가능 아이템이다.
- [ ] 모든 아이템은 강화 실패 시 파괴한다.
- [ ] 모든 아이템의 모든 강화 단계 별 성공 확률은 30% 다.
- [ ] 강화 성공 시 무기는 공격, 방어구는 방어 수치가 올라가야 한다. (상승 수치는 현재 값의 10%)
- [ ] 유저의 아이템 별로 강화 단계 이력 정보가 있어야한다.
## 던전 스테이지
- [ ] 스테이지 단계 순서대로 클리어 해야 한다.
- [ ] 클라이언트에서 스테이지 탭을 누르면 완료한 스테이지 리스트를 서버에 요청하고, 서버는 응답한다.
- [ ] 클라이언트에서 스테이지를 선택한 후 서버에 요청하면 서버는 선택 가능한지 검증한다.
- [ ] 던전에 생성될 아이템을 리스트를 보낸다.
- [ ] 적 NPC 리스트를 보낸다.
- [ ] 클라이언트는 아이템을 찾으면(파밍) 바로 서버에 알려준다.
- [ ] 스테이지를 클리어 해야만 찾은 아이템을 보상으로 준다
- [ ] NPC 전투는 클라이언트에서 실행되고, 결과를 서버에 보낸다.
- [ ] 처치한 NPC 수와 종류에 따라서 받는 경험치 등이 달라진다.
- [ ] 클라이언트는 NPC 한 마리 잡을 때 마다 서버로 송신.
- [ ] 스테이지가 완료되면, (서버가) 보상을 준다.
- [ ] 스테이지 클리어 조건은 스테이지에서 나오는 모든 몬스터를 다 잡아야한다.
- [ ] 마스터데이터의 스테이지_아이템 , 스테이지_공격NPC 를 참고한다.
## 채팅
- [ ] 웹서버만으로 채팅을 구현한다
- [ ] 채팅은 로비 단위로 구분된다. 로비 번호는 1~ 100개
- [ ] 유저는 로그인 시에 자동으로 로비에 입장된다.
- [ ] 유저는 로비 번호를 선택해서 지정한 로비에 입장 가능하다.
- [ ] 채팅 창을 통해서 이전 채팅 히스토리를 볼 수 있어야 한다. 단 현재 채팅에서 50개까지만 가능하다.
- [ ] Redis를 사용하여 구현한다.
