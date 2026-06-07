namespace BattleArena.Application.Services.Combat;

using Core.Entities;

internal sealed record ActorSetup(IAttackSource Source, Character Target, int TmCost, bool IsSpell);
