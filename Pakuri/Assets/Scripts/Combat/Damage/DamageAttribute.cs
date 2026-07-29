/*
 * 역할: 전투 피해 속성 분류.
 * 책임: 방어력 선택과 스킬 상호작용에 사용하는 피해 속성을 정의한다.
 */

namespace Pakuri.Combat
{

    /// <summary><c>DamageAttribute</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum DamageAttribute
    {
        Physical,
        Fire,
        Lightning,
        Ice,
        Darkness,
        Holy
    }
}
