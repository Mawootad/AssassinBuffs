using AssassinBuffs;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;
using System;
using System.Collections.Generic;
using UnityEngine;

class ClassesWithGuid
{
	public static List<(Type, string)> Classes = new()
		{
			(typeof(DamagePercentMultiplierModifierInitiator), "ce35a5f769c481c2baf23af65f3d4c6f"),
		};
}

namespace AssassinBuffs
{
	[TypeId("ce35a5f769c481c2baf23af65f3d4c6f")]
	[Serializable]
	public class DamagePercentMultiplierModifierInitiator : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDamage>
	{
		[SerializeField]
		private RestrictionCalculator Restrictions = new();

		[SerializeField]
		private ContextValueModifier PercentDamageMultiplier = new();

		[SerializeField]
		public ModifierDescriptor ModifierDescriptor;

		[SerializeField]
		private bool ModifyEvenDirectDamage;

		[SerializeField]
		private bool ModifyEvenDamageOverTime;

		public void OnEventAboutToTrigger(RuleCalculateDamage rule)
		{
			if (!Restrictions.IsPassed(Fact, rule, rule.Ability))
			{
				return;
			}
			if (rule.Unmodifiable)
			{
				return;
			}
			if (rule.DamageType == DamageType.Direct && !ModifyEvenDirectDamage)
			{
				return;
			}
			if (!ModifyEvenDamageOverTime && rule.Reason.Fact != null)
			{
				BlueprintBuff blueprintBuff = ((rule.Reason.Fact.Blueprint is BlueprintBuff) ? (rule.Reason.Fact.Blueprint as BlueprintBuff) : null);
				if (blueprintBuff != null && blueprintBuff.AbilityGroups.Contains(BlueprintWarhammerRoot.Instance.CombatRoot.DamageOverTimeAbilityGroup))
				{
					return;
				}
			}
			using (ContextData<PropertyContextData>.Request().Setup(new(rule.ConcreteInitiator, Fact, rule.MaybeTarget, Context, rule, rule.Ability)))
			{
				rule.ValueModifiers.Add(ModifierType.PctMul, PercentDamageMultiplier.Calculate(Context), Fact, ModifierDescriptor);
			}
		}

		public void OnEventDidTrigger(RuleCalculateDamage evt) { }
	}
}
