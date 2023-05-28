# com2usServerCamp 연규준
컴투스 서버캠프 과제 - 던전파밍

## 배포 방식
1. ./deployment 파일을 배포할 서버로 복사
2. ./DungeonFarming/DungeonFarming에서 컨테이너 이미지 빌드
(`dotnet publish --os linux --arch 아키텍쳐 -p:PublishProfile=DefaultContainer -c Release`)
3. 빌드한 이미지를 배포할 서버로 이동(`docker save`, `docker load`이용)
4. `docker-compose up`으로 서버를 올림.

아래 블로그에 새부 내용이 있다.
https://blog-for-sw-study.tistory.com/64

## 과제 진행 방식
main branch를 기점으로, 새로운 기능 추가시
1. 이슈를 판다
2. 이슈에 대응하는 branch를 main으로 부터 만든다
3. 기능 구현 후 main에 merge한다
