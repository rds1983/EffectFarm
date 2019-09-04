#pragma once

#include <d3dcompiler.h>
#include <vector>
#include <memory>
#include <vcclr.h>

#pragma comment(lib,"d3dcompiler.lib")

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

namespace EffectFarm {
	struct CharHolder
	{
		char *str;

		CharHolder(String ^data)
		{
			str = (char*)(void*)Marshal::StringToHGlobalAnsi(data);
		}

		~CharHolder()
		{
			if (str != nullptr)
			{
				Marshal::FreeHGlobal(IntPtr((void *)str));
			}
		}

		operator char *()
		{
			return str;
		}
	};

	struct CharHolders
	{
		std::vector<std::shared_ptr<CharHolder>> holders;

		char *Add(String ^str)
		{
			holders.push_back(std::shared_ptr<CharHolder>(new CharHolder(str)));

			return (char *)*holders.back();
		}
	};

	public ref class D3DCompiler
	{
	public:
		static array<unsigned char> ^Compile(String ^path, 
			Dictionary<String ^, String ^> ^defines,
			String ^target)
		{
			UINT flags = D3DCOMPILE_ENABLE_STRICTNESS;

			CharHolders charHolders;

			// Convert defines to macroses
			std::vector<D3D_SHADER_MACRO> macroses;
			for each(auto pair in defines)
			{
				D3D_SHADER_MACRO macro;

				macro.Name = charHolders.Add(pair.Key);
				macro.Definition = charHolders.Add(pair.Value);
				macroses.push_back(macro);
			}

			// End
			macroses.push_back(D3D_SHADER_MACRO());

			pin_ptr<const wchar_t> wPath = PtrToStringChars(path);

			LPCSTR aTarget = charHolders.Add(target);

			// Compile
			ID3DBlob* shaderBlob = nullptr;
			ID3DBlob* errorBlob = nullptr;

			HRESULT hr = D3DCompileFromFile(wPath, &macroses[0],
				D3D_COMPILE_STANDARD_FILE_INCLUDE,
				NULL,
				aTarget,
				flags,
				0,
				&shaderBlob,
				&errorBlob);

			if (FAILED(hr))
			{
				String ^err = String::Empty;
				if (errorBlob)
				{
					err = gcnew String((char*)errorBlob->GetBufferPointer());
					errorBlob->Release();
				}

				if (shaderBlob)
				{
					shaderBlob->Release();
				}

				throw gcnew Exception(err);
			}

			if (errorBlob)
			{
				errorBlob->Release();
			}


			array<unsigned char> ^result;
			if (shaderBlob)
			{
				result = gcnew array<unsigned char>(shaderBlob->GetBufferSize());

				Marshal::Copy(IntPtr(shaderBlob->GetBufferPointer()), 
					result, 
					0, 
					result->Length);

				shaderBlob->Release();
			}

			return result;
		}
	};
}
