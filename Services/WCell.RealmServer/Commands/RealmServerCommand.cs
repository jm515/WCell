using WCell.Constants.Updates;
using WCell.Util.Commands;
using WCell.Intercommunication.DataTypes;

namespace WCell.RealmServer.Commands
{
	public abstract class RealmServerCommand : Command<RealmServerCmdArgs>
	{

		/// <summary>
		/// The kind of target that is required for this command 
		/// (Target is set to the command-calling User, if he/she has none selected or not doubled the Command-Prefix).
		/// </summary>
		public virtual ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.None; }
		}

		public virtual bool RequiresContext
		{
			get { return NeedsCharacter || TargetTypes != 0; }
		}

		/// <summary>
		/// Whether the Character argument needs to be supplied by the trigger's Args
		/// </summary>
		public virtual bool NeedsCharacter
		{
			get { return false; }
		}

		/// <summary>
		/// Whether the command-user must be of equal or higher rank of the target.
		/// Used to prevent staff members of lower ranks to perform any kind
		/// of unwanted commands on staff members of higher ranks.
		/// </summary>
		public virtual bool RequiresEqualOrHigherRank
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// The status that is required by a Command by default
		/// </summary>
		public virtual RoleStatus RequiredStatusDefault
		{
			// make Staff default
			get { return RoleStatus.Staff; }
		}

		public void ProcessNth(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var no = trigger.Text.NextUInt(1);
			var subAlias = trigger.Text.NextWord();

			BaseCommand<RealmServerCmdArgs>.SubCommand subCmd;
			if (m_subCommands.TryGetValue(subAlias, out subCmd))
			{
				trigger.Args = new RealmServerNCmdArgs(trigger.Args, no);
				if (MayTrigger(trigger, subCmd, false))
				{
					subCmd.Process(trigger);
				}
			}
			else
			{
				trigger.Reply("SubCommand not found: " + subAlias);
				trigger.Text.Skip(trigger.Text.Length);
				mgr.DisplayCmd(trigger, this);
			}
		}

		public override bool MayTrigger(CmdTrigger<RealmServerCmdArgs> trigger, BaseCommand<RealmServerCmdArgs> cmd, bool silent)
		{
			if (!(cmd is SubCommand) || trigger.Args.User == null ||
				   trigger.Args.Role.Status >= ((SubCommand)cmd).DefaultRequiredStatus)
			{
				return true;
			}
			else if (!silent)
			{
				trigger.Reply("You are not allowed to use that Command.");
			}
			return false;
		}

		public abstract new class SubCommand : BaseCommand<RealmServerCmdArgs>.SubCommand
		{
			public virtual RoleStatus DefaultRequiredStatus
			{
				get { return ((RealmServerCommand)m_parentCmd).RequiredStatusDefault; }
			}
		}
	}
}