#pragma once

#include <d3dcompiler.h>
#include <vector>
#include <vcclr.h>

#pragma comment(lib,"d3dcompiler.lib")

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

namespace EffectFarm {
	public ref class D3DCompiler
	{
	public:
		static array<unsigned char> ^Compile(String ^path, 
			Dictionary<String ^, String ^> ^defines,
			String ^target)
		{
			UINT flags = D3DCOMPILE_ENABLE_STRICTNESS;

			// Convert defines to macroses
			std::vector<D3D_SHADER_MACRO> macroses;
			for each(auto pair in defines)
			{
				D3D_SHADER_MACRO macro;

				macro.Name = (char*)(void*)Marshal::StringToHGlobalAnsi(pair.Key);
				macro.Definition = (char*)(void*)Marshal::StringToHGlobalAnsi(pair.Value);
				macroses.push_back(macro);
			}

			// End
			macroses.push_back(D3D_SHADER_MACRO());

			pin_ptr<const wchar_t> wPath = PtrToStringChars(path);
			LPCSTR aTarget = (char*)(void*)Marshal::StringToHGlobalAnsi(target);

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

			// Free allocated strings
			for (size_t i = 0; i < macroses.size(); ++i)
			{
				if (macroses[i].Name)
				{
					Marshal::FreeHGlobal(IntPtr((void *)macroses[i].Name));
				}

				if (macroses[i].Definition)
				{
					Marshal::FreeHGlobal(IntPtr((void *)macroses[i].Definition));
				}
			}

			Marshal::FreeHGlobal(IntPtr((void *)aTarget));

			return nullptr;
		}
	};
}
