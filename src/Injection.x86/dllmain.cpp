#include "stdafx.h"
#include <iostream>
#include <stdint.h>

using namespace std;

void injection();

BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
	)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		injection();
		return FALSE;
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

int readStringLength(HANDLE pipe)
{
	int count = 0;
	int shift = 0;
	uint8_t b;

	do
	{
		ReadFile(pipe, &b, 1, NULL, NULL);
		count |= (b & 0x7F) << shift;
		shift += 7;
	} while ((b & 0x80) != 0);

	return count;
}

void writeStringLength(HANDLE pipe, int length)
{
	auto v = (uint32_t)length;
	uint8_t b;

	while (v >= 0x80) {
		b = (uint8_t)(v | 0x80);
		WriteFile(pipe, &b, 1, NULL, NULL);
		v >>= 7;
	}

	b = (uint8_t)v;

	WriteFile(pipe, &b, 1, NULL, NULL);
}

wchar_t * readString(HANDLE pipe)
{
	int length = readStringLength(pipe);
	auto value = new wchar_t[length + 1]();
	ReadFile(pipe, value, length * sizeof(wchar_t), NULL, NULL);

	return value;
}

void writeString(HANDLE pipe, wchar_t * str)
{
	auto length = wcslen(str);
	writeStringLength(pipe, length * sizeof(wchar_t));

	WriteFile(pipe, str, length * sizeof(wchar_t), NULL, NULL);
}

void setEnv(HANDLE pipe)
{
	auto variable = readString(pipe);
	auto value = readString(pipe);

	SetEnvironmentVariable(variable, value);

	delete variable;
	delete value;
}

void readEnv(HANDLE pipe)
{
	auto variable = readString(pipe);

	auto valueLength = GetEnvironmentVariable(variable, NULL, 0);

	wchar_t * value;

	if (valueLength == 0)
	{
		value = L"";
	}
	else
	{
		value = new wchar_t[valueLength]();
		GetEnvironmentVariable(variable, value, valueLength);
	}

	writeString(pipe, value);

	delete variable;
}

void pingPong(HANDLE pipe)
{
	uint8_t response = 54;
	WriteFile(pipe, &response, 1, NULL, NULL);
}

void injection()
{
	int i = 0;
	
	wchar_t pipeName[1024];

	wsprintf(pipeName, L"\\\\.\\pipe\\pather\\%d", GetCurrentProcessId());

	HANDLE pipe = CreateFile(pipeName, GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);

	while (true)
	{
		uint8_t commandCode;

		if (!ReadFile(pipe, &commandCode, 1, NULL, NULL))
		{
			if (GetLastError() == ERROR_BROKEN_PIPE)
			{
				break;
			}
		}

		switch (commandCode)
		{
		case 45:
			pingPong(pipe);
			break;
		case 01:
			setEnv(pipe);
			break;
		case 02:
			readEnv(pipe);
			break;
		}
	}
}