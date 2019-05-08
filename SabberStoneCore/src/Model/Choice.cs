﻿#region copyright
// SabberStone, Hearthstone Simulator in C# .NET Core
// Copyright (C) 2017-2019 SabberStone Team, darkfriend77 & rnilva
//
// SabberStone is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License.
// SabberStone is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
#endregion
using System.Collections.Generic;
using System.Text;
using SabberStoneCore.Enums;
using SabberStoneCore.Model.Entities;
using System;
using SabberStoneCore.Tasks;
using SabberStoneCore.Tasks.SimpleTasks;

namespace SabberStoneCore.Model
{
	/// <summary>
	/// 
	/// </summary>
	public enum ChoiceAction
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		ADAPT, HAND, SUMMON, HEROPOWER, KAZAKUS, TRACKING, INVALID, SPELL_RANDOM, GLIMMERROOT, BUILDABEAST, CAST, STACK,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}

	/// <summary>
	/// 
	/// </summary>
	public class Choice
	{
		/// <summary>Initializes a new instance of the <see cref="Choice"/> class.</summary>
		/// <param name="controller">The controller.</param>
		/// <autogeneratedoc />
		public Choice(Controller controller)
		{
			Controller = controller;
		}

		public Choice(Controller controller, Card[][] cardSet)
		{
			Controller = controller;
			_cardSets = cardSet;
		}

		private Choice(Controller controller, Choice choice)
		{
			Controller = controller;
			_cardSets = choice._cardSets;
			ChoiceType = choice.ChoiceType;
			ChoiceAction = choice.ChoiceAction;
			if (choice.Choices != null)
				Choices = new List<int>(choice.Choices);
			SourceId = choice.SourceId;
			LastChoice = choice.LastChoice;
			AfterChooseTask = choice.AfterChooseTask;
			if (choice.EntityStack != null)
				EntityStack = new List<int>(choice.EntityStack);
			if (choice.NextChoice != null)
				NextChoice = new Choice(Controller, choice.NextChoice);
		}

		/// <summary>Gets or sets the controller making the choice.</summary>
		/// <value>The controller.</value>
		public Controller Controller { get; set; }

		/// <summary>Gets or sets the type of choice.</summary>
		/// <value><see cref="Enums.ChoiceType"/></value>
		public ChoiceType ChoiceType { get; set; } = ChoiceType.INVALID;

		/// <summary>Gets or sets the choice action.</summary>
		/// <value>The choice action.</value>
		public ChoiceAction ChoiceAction { get; set; } = ChoiceAction.INVALID;

		/// <summary>Gets or sets the IDs of the selected entities.</summary>
		/// <value>The chosen entity IDs.</value>
		public List<int> Choices { get; set; }

		/// <summary>Gets or sets the ID of the entity which produced this choice.</summary>
		/// <value>The entity ID.</value>
		public int SourceId { get; set; }

		/// <summary>Gets or sets the IDs of the entities to choose from.</summary>
		/// <value>The entity IDs.</value>
		//public List<int> TargetIds { get; set; }

		/// <summary>Gets or sets the remaining choices for the controller.</summary>
		//public Queue<Choice> ChoiceQueue { get; set; } = new Queue<Choice>();

		/// <summary>Gets or sets the ID of the last chosen entity.</summary>
		/// <value>The entity Id.</value>
		public int LastChoice { get; set; }

		/// <summary>Gets or sets the card of enchantment that must be applied after the entity is chosen.</summary>
		//public Card EnchantmentCard { get; set; }

		/// <summary>Gets or sets the task that must be done after the entity is chosen.</summary>
		public ISimpleTask AfterChooseTask { get; set; }

		/// <summary>Gets or sets the next consecutive choice.</summary>
		public Choice NextChoice { get; set;}

		//internal IList<IPlayable> EntityStack { get; set; }
		internal List<int> EntityStack { get; set; }

		private readonly Card[][] _cardSets;

		internal void TryPrepare()
		{
			if (_cardSets == null) return;

			Card[] cards = DiscoverTask.GetChoices(_cardSets, 3);
			var choices = new List<int>(3);
			foreach (Card card in cards)
				choices.Add(Entity.FromCard(Controller, card,  new EntityData
				{
					{GameTag.CREATOR, SourceId},
					{GameTag.DISPLAYED_CREATOR, SourceId}
				},Controller.SetasideZone).Id);

			Choices = choices;
		}

		internal void AddToStack(int entityId)
		{
			if (EntityStack == null)
				EntityStack = new List<int> {entityId};
			else
				EntityStack.Add(entityId);

			//if (EntityStack is List<IPlayable> list)
			//	list.Add(entity);
			//else if (EntityStack != null)
			//{
			//	list = new List<IPlayable>(EntityStack) {entity};
			//	EntityStack = list;
			//}
			//else
			//{
			//	list = new List<IPlayable> {entity};
			//	EntityStack = list;
			//}

		}

		internal bool TryPopNextChoice(int lastChoice, out Choice nextChoice)
		{
			nextChoice = NextChoice;
			if (nextChoice == null)
				return false;
			nextChoice.LastChoice = lastChoice;
			nextChoice.TryPrepare();
			nextChoice.EntityStack = EntityStack;
			return true;
		}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public Choice Clone(Controller controller)
		{
			return new Choice(controller, this);
		}

		public string FullPrint()
		{
			var str = new StringBuilder();
			str.Append($"{Controller.Name}[ChoiceType:{ChoiceType}][ChoiceAction:{ChoiceAction}][");
			str.Append(String.Join(",", Choices));
			return str.ToString();
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
