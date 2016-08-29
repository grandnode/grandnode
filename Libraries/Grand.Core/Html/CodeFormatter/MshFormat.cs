
namespace Grand.Core.Html.CodeFormatter
{
	/// <summary>
	/// Generates color-coded HTML 4.01 from MSH (code name Monad) source code.
	/// </summary>
    public partial class MshFormat : CodeFormat
	{
		/// <summary>
		/// Regular expression string to match single line comments (#).
		/// </summary>
		protected override string CommentRegex
		{
			get { return @"#.*?(?=\r|\n)"; }
		}

		/// <summary>
		/// Regular expression string to match string and character literals. 
		/// </summary>
		protected override string StringRegex
		{
			get { return @"@?""""|@?"".*?(?!\\).""|''|'.*?(?!\\).'"; }
		}

		/// <summary>
		/// The list of MSH keywords.
		/// </summary>
		protected override string Keywords 
		{
			get 
			{ 
				return "function filter global script local private if else"
					+ " elseif for foreach in while switch continue break"
					+ " return default param begin process end throw trap";
			}
		}

		/// <summary>
		/// Use preprocessors property to hilight operators.
		/// </summary>
		protected override string Preprocessors
		{
			get
			{
				return "-band -bor -match -notmatch -like -notlike -eq -ne"
					+ " -gt -ge -lt -le -is -imatch -inotmatch -ilike"
					+ " -inotlike -ieq -ine -igt -ige -ilt -ile";
			}
		}

	}
}
