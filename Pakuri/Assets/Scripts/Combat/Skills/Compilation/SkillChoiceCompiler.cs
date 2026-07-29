using Pakuri.Data;

/*
 * Enhancement와 Master 선택지를 전투용 데이터와 실행 노드로 변환한다.
 * 스킬 전체 변환을 조율하는 SkillDefinitionCompiler와 달리 선택지 필드 변환만 담당한다.
 */
namespace Pakuri.InGame
{
    internal static class SkillChoiceCompiler
    {
	/*
	 * Compile 작업 결과를 반환한다.
	 */
	internal static SkillChoice[] Compile(SkillChoiceDefinition[] source /* 변환할 스킬 선택지 정의 목록 */)
	{
		SkillChoice[] array = new SkillChoice[source.Length];
		for (int i = 0; i < source.Length; i++)
		{
			SkillChoiceDefinition skillChoiceDefinition = source[i];
			array[i] = new SkillChoice
			{
				ChoiceId = skillChoiceDefinition.ChoiceId,
				MonsterId = skillChoiceDefinition.MonsterId,
				SkillId = skillChoiceDefinition.SkillId,
				TargetSkillId = skillChoiceDefinition.TargetSkillId,
				ChoiceGroup = skillChoiceDefinition.ChoiceGroup,
				Title = skillChoiceDefinition.Title,
				SkillIcon = skillChoiceDefinition.SkillIcon,
				SkillEffectPrefab = skillChoiceDefinition.SkillEffectPrefab,
				DescriptionText = skillChoiceDefinition.DescriptionText,
				Nodes = SkillNodeMapper.MapSkillNodeDefinitions(skillChoiceDefinition.NormalizedNodes)
			};
		}
		return array;
	}

    }
}
