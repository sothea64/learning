using System;					
public class Program
{
	public static void Main()
	{
		/*
		 Currently I can only make it to calculate correctly with the increment
		 10*(10*n), anything outside that is not correct
		*/
		bool roundUp = true;
		decimal input = 10001;
		int step = 100;
		decimal roundedPart = (input/step) % step;
		Console.WriteLine("roundedPart: "+roundedPart.ToString());
		decimal conditionRound = roundedPart % 1;
		if (conditionRound > 0)
		{
			Console.WriteLine("Need To Round: "+conditionRound.ToString());
			int addOn = 0;
			string roundStr = string.Empty;
			if (roundUp)
			{
				addOn = step - (int)(conditionRound * step);
				roundStr = "Round UP";
				Console.WriteLine("Value Need To Round UP: "+addOn.ToString());
			}
			else
			{
				addOn = (step + (int)(conditionRound * step)) * -1;
				roundStr = "Round DOWN";
				Console.WriteLine("Value Need To Round DOWN: "+addOn.ToString());
			}
			input = input + addOn;
			Console.WriteLine("Value "+roundStr+" :"+input.ToString());
		}
		else
		{
			Console.WriteLine("No Need To Round: "+conditionRound.ToString());
		}
	}
}
