using System;
using System.Threading.Tasks;

namespace MyLib 
{
	public class TeamManageCom : Component
	{
		private int teamColor = 0;
		public TeamManageCom ()
		{
		}
		public async Task<int> GetTeamColor() {
			await this.actor._messageQueue;
			//var tc = teamColor % 2;
		    var tc = teamColor;
			teamColor++;
			return tc;
		}
	}
}

