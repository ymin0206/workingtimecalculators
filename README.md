# 근무시간 계산기
이달 필수 근무시간 대비 현재까지의 근무시간을 분석하고,
남은 기간 동안 매일 몇 시에 출근해야 하는지 계산해주는 Windows 데스크탑 앱입니다.

## 주요 기능
- 오늘 날짜 기준 출근일 자동 계산 (공휴일/대체공휴일 반영)
- 미래 연차 입력 시 8시간으로 자동 환산
- 남은 근무일 기준 하루 필요 근무시간 및 권장 출근 시각 계산
- **이달 마지막 날인 경우** 오늘 퇴근 가능 시각 표시 (점심 1시간 포함)
- 공휴일 자동 갱신 (data.go.kr 공식 API 또는 date.nager.at)
- 기준 출근 시각 직접 수정 가능

## 사용 방법
1. 근무시간계산기.exe 실행
2. 총 근로 시간 입력 (예: 151:44)
3. 미래 연차 개수 입력
4. **계산** 버튼 클릭
> 처음 실행 시 TotalTimeList.txt가 자동 생성됩니다.  
> **공휴일 자동 갱신** 버튼으로 해당 연도 공휴일을 반영한 근무일/필수시간을 업데이트할 수 있습니다.

## 실행 환경
- Windows 10/11 x64
- .NET 런타임 불필요 (자체 포함 단일 실행파일)

## 빌드
```bash
dotnet publish -c Release -r win-x64 --self-contained \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -o publish
