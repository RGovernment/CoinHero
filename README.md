# CoinHero

<img width="1772" height="994" alt="전투화면_선택" src="https://github.com/user-attachments/assets/6e594299-21e8-482e-a2bd-7458156aeed9" />


1. 프로젝트 개요
   - 게임명 : 코인 히어로
   - 게임 개요 : 덱빌딩 로그라이크
   - 한줄 정의 : 카드로 적과 합을 겨뤄 승리하고 더 강한 카드로 진화해 보스를 물리치자.
2. 실행 방법
   - 빌드 : CoinHero.exe 실행
   - Unity 버전 : 6000.3.13f1
   - 조작키 : 마우스 조작
3. 핵심 플레이 흐름
   - 타이틀 화면 → 맵 선택 → (전투/휴식처/상점) 화면 → (전투일 시) 보상 화면 → 맵 선택 → ... → 보스 맵 → 타이틀 화면
4. 주요 기능 목록
   - 비동기 처리를 위한 UniTask, 세이브, 로드르 위한 Json/AES 암호화, 턴 관리를 위한 유한 상태 머신(FSM)
5. 프로젝트 구조
   - 씬 구조 : MainTitleScene, LoadingScene, BattleScene, MapScene, ShopScene
- 스크립트 폴더 구조 및 주요 스크립트(폴더만 전체 표시, 스크립트는 주요 스크립트만 남김)
<details>
<summary><b> 핵심 폴더 구조 열기 </b></summary>

```text
Script/
│  
├─Card/
│  ├─Base/
│  │      Card.cs
│  │      CardData.cs
│  │      
│  ├─CardTest/
│  │      
│  └─Main/
│          DeckManager.cs
│          EnemyHandManager.cs
│          HandManager.cs
│          OtherBattleDeckManager.cs
│          
├─Common/
│  ├─Animation/
│  │      
│  ├─Combat/
│  │      CombatAnimatorManager.cs
│  │      CombatBase.cs
│  │      
│  ├─Sound/
│  │      
│  └─Text/
│          HyperLink.cs
│          ToolTipManager.cs
│          
├─Core/
│  ├─Common/
│  │      
│  ├─Data/
│  │      OptionData.cs
│  │      SaveData.cs
│  │      
│  ├─Managing/
│  │      BattleManager.cs
│  │      GameManager.cs
│  │      ResourceManager.cs
│  │      SaveManager.cs
│  │      
│  ├─Model/
│  │      Character.cs
│  │      IBuffable.cs
│  │      ICombat.cs
│  │      IDamageable.cs
│  │      IInstantEffect.cs
│  │      IResourceControl.cs
│  │      IState.cs
│  │      
│  └─State/
│          
├─Effect/
│  ├─Buff/
│  │      
│  └─Debuff/
│          
├─Enemy/
│  └─Main/
│          Enemy.cs
│          EnemyCombat.cs
│          
├─Map/
│  ├─Base/
│  │      MapConfig.cs
│  │      MapCreater.cs
│  │      
│  └─Main/
│          MapManager.cs
│          
├─Other/
│  ├─RestArea/
│  │      
│  └─Shop/
│          
├─Player/
│  ├─Main/
│  │      Player.cs
│  │      PlayerCombat.cs
│  │      
│  └─Status/
│          
├─Title/
│      TitleManager.cs
│      
└─UI/
    ├─Battle/
    │      
    └─Option/
            EffectTextParser.cs
```
</details>

6. 핵심 로직 소개
핵심 로직 : 전투
<p>
<img width="600" height="200" alt="image" src="https://github.com/user-attachments/assets/a44241ef-ac7d-4bab-892f-d32b4e1b2439" />
  </p>
행동할 플레이어와 적의 카드를 Queue로 적재하고, 해당 Queue에 맞춰 행동, 행동에 따라 합/일방 행동을 사용하여 행동마다 각각의 상황에 맞는 값을 부여함.  
  
9. UniTask, Json/AES 암호화
  
  
UniTask 선택 이유 : struct값으로 이루어져 코루틴에 비해 GC(가비지 커넥터)가 매우 적고, 호출 및 사용, 값 반환 등의 다양한 사용에 제한이 적음.  
Json/AES 암호화 선택 이유 : Json 데이터 자체가 관리가 쉬우며 NewtonSoft에서 제공하는 라이브러리로 인해 사용이 편함.  
                           AES 암호화는 사용자의 간단한 세이브 파일 변조를 1차적으로 막기 위함.
  
11. 코루틴을 사용하지 않고 UniTask를 사용하므로써 게임 내 Coin 작동을 위해 자주 사용하는 비동기 애니메이션 연출에 메모리 소모량이 감소함.
12. 생겼던 문제점
- 데이터 저장 시, MapGraph의 데이터가 Json으로 변환되지 않고 저장이 멈추는 오류

원인 : MapGraph 데이터 내의 MapNode 객체가 이어진 Node와의 정보를 유지하기 위해 List<MapNode> 내에 다른 MapNode 데이터를 저장하고 있었고, 해당 객체가 서로 순환 참조되어 오류가 발생함.

해결 : List 저장을 배제하고, 저장용 MapGraphData와 MapNodeData 객체를 생성해 저장/로드 시 변환하여 사용할 수 있도록 함
10. 개선점 
몇몇 기능이 기획과 달리 시간의 부족으로 인해 부재하였으나 해당 부분은 보강하여 추가하고 싶음.
