﻿/**
 * Este código genera un comando ejecutable que permite usar la API de SuperFactura a través
 * de parámetros pasados por linea de comandos.
 * Este comando permite, por ejemplo, ser invocado desde programas escritos en otros lenguajes
 * y hacer una integración a través de archivos JSON o XML.
 */

using System;
using SuperFactura;
using Newtonsoft.Json;

namespace SuperFactura_API_Command
{
	class Program
	{
		static void Main(string[] args)
		{
			// TODO: Add SF_API_PLATFORM = '-dos-net';

			if (args.Length < 3)
			{
				Console.Error.WriteLine(@"
Envia un archivo DTE en formato JSON al SII.

SUPERFACTURA [usuario] [contraseña] [ambiente] [archivo] [opciones]

  usuario	: Correo electrónico de su cuenta de SuperFactura.
  contraseña	: Contraseña de su cuenta de SuperFactura.
  ambiente	: Usar 'cer' para certificación o 'pro' para producción.
  archivo	: Archivo en formato JSON o XML con los datos del DTE.
  opciones	: Opcional. Cadena en formato JSON con opciones adicionales.

Documentación: https://superfactura.cl/pages/otros-lenguajes
Envíe sus consultas a: soporte@superfactura.cl
				");
				Environment.Exit(1);
			}

			string user = args[0];
			string pass = args[1];
			string ambiente = args[2];
			string dteFile = null;
			string optionsJSON = null;
			if (args.Length >= 4) dteFile = args[3];
			if (args.Length >= 5) optionsJSON = args[4];

			API api;
			dynamic options = null;

			if (optionsJSON != null)
			{
				options = JsonConvert.DeserializeObject(optionsJSON);
			}

			if (options.host != null)
			{
				// Conexión a Servidor Local
				api = new API((string)options.host, 9080);
			} else
			{
				// Conexión a la nube
				api = new API(user, pass);
			}

			string printer = null;
			if (options != null) {
				if (options.savePDF != null)
				{
					api.SetSavePDF((string)options.savePDF);
				}

				printer = GetAndRemove(options, "printer");
				if (printer != null)
				{
					options["getEscPos"] = 1;
				}
			}

			if (optionsJSON != null)
			{
				optionsJSON = JsonConvert.SerializeObject(options);
				api.AddOptions(optionsJSON);
			}

			string json = System.IO.File.ReadAllText(dteFile);

			try
			{
				APIResult res = api.SendDTE(json, ambiente);
				// Console.WriteLine("Se creó el DTE con folio " + res.folio);
				// {"ok":true,"folio":"125"}
				Console.WriteLine("{\"ok\":" + (res.ok ? "true" : "false") + ",\"folio\":\"" + res.folio + "\"}");

				if(printer != null) res.PrintEscPos(printer);
			}
			catch (Exception e)
			{
				// IMPORTANTE: Este mensaje se debe mostrar al usuario para poder darle soporte.
				Console.WriteLine(e.Message);
			}
		}

		static dynamic GetAndRemove(dynamic obj, string key)
		{
			dynamic res = obj[key];
			if (res != null) obj.Remove(key);
			return res;
		}
	}
}