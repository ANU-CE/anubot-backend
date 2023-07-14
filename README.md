# anubot-backend
## 소개
아누봇의 백엔드 API 서버입니다.

## API 문서
https://anubot.azurewebsites.net

## 코딩 표준
* DateTime 포맷은 UTC 시간대를 사용합니다.
* secret(ex. API키, DB 연결 정보)은 git에 저장하지 않습니다. 대신 다음과 같은 설정 파일을 로컬에서 생성하여 관리합니다.
  - `appsettings.Production.json` - 운영
  - `appsettings.Development.json` - 개발 및 디버그

# 배포
* Jenkins

## 참고 문서
* ASP.NET 공식 문서: https://learn.microsoft.com/ko-kr/aspnet/core/?view=aspnetcore-7.0
* OpenAI API 공식 문서: https://platform.openai.com/docs/introduction
