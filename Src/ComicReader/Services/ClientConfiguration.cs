using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Skinnix.ComicReader.Client.Services
{
	interface IClientConfiguration
	{
		public string ServerUrl { get; }
		public string? DisplayName { get; }
	}

	class ClientConfiguration : IClientConfiguration
	{
		public string ServerUrl { get; set; }
		public string? DisplayName { get; set; }

#pragma warning disable CS8618 //possible null value in non-nullable field
		public ClientConfiguration(IConfiguration config)
		{
			config.Bind(this);

			CheckIntegrity();
		}
#pragma warning restore CS8618 //possible null value in non-nullable field

		private void CheckIntegrity()
		{
			foreach (var property in GetType().GetProperties())
			{
				var value = property.GetValue(this);
				if (property.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute") == null)
					if (value == null)
						throw new ConfigurationErrorsException($"Eigenschaft \"{property.Name}\" darf nicht leer sein");
			}
		}
	}
}
