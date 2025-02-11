#include "AddObjectDialog.hpp"
#include <Engine3DRadSpace/Logging/Error.hpp>
#include "resource.h"
#include <CommCtrl.h>

//Forward declarations of object reflection data
REFL_DEF(Camera)
REFL_DEF(Sprite)
REFL_DEF(Skinmesh)

using namespace Engine3DRadSpace;
using namespace Engine3DRadSpace::Reflection;

std::vector<std::pair<Reflection::UUID, ReflectedObject *>> AddObjectDialog::Objects;

INT_PTR WINAPI AddObjectDialog_DlgProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	static AddObjectDialog* aod = nullptr;
	switch (msg)
	{
		case WM_INITDIALOG:
		{
			aod = reinterpret_cast<AddObjectDialog*>(lParam);
			aod->window = hwnd;
			aod->createForms();
			return 1;
		}
		case WM_PAINT:
		{
			PAINTSTRUCT ps;
			BeginPaint(hwnd, &ps);
			FillRect(ps.hdc, &ps.rcPaint, (HBRUSH)(COLOR_WINDOW + 1));
			EndPaint(hwnd, &ps);
			return 1;
		}
		case WM_CLOSE:
		{
			EndDialog(hwnd, IDCANCEL);
			return 1;
		}
		case WM_SIZE:
		{
			aod->resize();
			return 1;
		}
		case WM_COMMAND:
		{
			return 1;
		}
		case WM_NOTIFY:
		{
			switch((reinterpret_cast<LPNMHDR>(lParam))->code)
			{
				case NM_DBLCLK:
				{
					LPNMITEMACTIVATE item = reinterpret_cast<LPNMITEMACTIVATE>(lParam);

					if(item->iItem >= 0)
					{
						EditObjectDialog dialog(hwnd, aod->hInstance, aod->Objects[item->iItem].second, aod->_content);
						EndDialog(hwnd, reinterpret_cast<INT_PTR>(dialog.ShowDialog()));
					}
					break;
				}
				default:
					break;
			}
			return 1;
		}
		default:
			return 0;
	}
}

void AddObjectDialog::addCategory(const std::string& category,int index)
{
	LVGROUP group{};
	group.cbSize = sizeof(LVGROUP);
	group.mask = LVGF_HEADER | LVGF_GROUPID;

	std::wstring str = ConvertToWideString(category).c_str();
	group.pszHeader = const_cast<wchar_t*>(str.c_str());
	group.cchHeader = int(str.length());
	group.iGroupId = index;

	ListView_InsertGroup(listView, index, &group);
}

void AddObjectDialog::addObject(const std::string& objectName, int imageIndex, int groupIndex)
{
	LVITEMW item{};

	std::wstring itemName = ConvertToWideString(objectName);
	item.pszText = const_cast<wchar_t*>(itemName.c_str());

	item.cchTextMax = int(itemName.size());
	item.iGroupId = groupIndex;
	item.iImage = imageIndex;
	item.iItem = itemIndex++;
	item.mask = LVIF_IMAGE | LVIF_TEXT | LVIF_GROUPID;

	ListView_InsertItem(listView, &item);
}

AddObjectDialog::AddObjectDialog(HWND owner_window, HINSTANCE instance, Engine3DRadSpace::Content::ContentManager* content):
	Dialog(owner_window, instance, AddObjectDialog_DlgProc, "Add a object..."),
	imageList(nullptr),
	listView(nullptr),
	_content(content)
{
}

Engine3DRadSpace::Reflection::ReflectedObject *AddObjectDialog::GetReflDataFromUUID(const Engine3DRadSpace::Reflection::UUID &uuid)
{
	for(auto &[obj_uuid, refl] : Objects)
	{
		if(obj_uuid == uuid)
			return refl;
	}
	return nullptr;
}

Engine3DRadSpace::IObject* AddObjectDialog::ShowDialog()
{
	return reinterpret_cast<Engine3DRadSpace::IObject *>(Dialog::ShowDialog(static_cast<void *>(this)));
}
struct objectItem
{
	LPCWSTR image;
	int categoryID;
};

void AddObjectDialog::createForms()
{
	//Create the list view control
	listView = CreateWindowExA(0, "SysListView32", "", WS_VISIBLE | WS_CHILD | LVS_ALIGNTOP, 0, 0, 600, 600, window, nullptr, hInstance, nullptr);
	if (listView == nullptr) throw std::exception("Failed to create a list view control!");
	ListView_EnableGroupView(listView, true);

	//Create the image list
	imageList = ImageList_Create(64, 64, ILC_COLOR32, 20, 5);
	if (imageList == nullptr) throw std::exception("Failed to create a image list!");

	ListView_SetImageList(listView, imageList, LVSIL_NORMAL); //Assign the image list to the list view

	std::unordered_map<std::string, objectItem> objects;
	std::unordered_map<std::string, int> categories;

	//hardcoded list of already implemented objects.
	if(Objects.size() == 0)
	{
		Objects =
		{
			{CameraReflInstance.ObjectUUID,&CameraReflInstance}, //Camera
			/*
			&CameraReflInstance, //Counter
			&CameraReflInstance, //Empty
			&CameraReflInstance, //Event On Key
			&CameraReflInstance, //EventOnLocation
			&CameraReflInstance, //ExitFade
			&CameraReflInstance, //Fog
			&CameraReflInstance, //Force
			&CameraReflInstance, //FPVCamera
			&CameraReflInstance, //Settings
			&CameraReflInstance, //G-Force
			&CameraReflInstance, //Group
			&CameraReflInstance, //Network chat
			&CameraReflInstance, //Rigidbody
			&CameraReflInstance, //C# Script
			*/
			{SkinmeshReflInstance.ObjectUUID, &SkinmeshReflInstance}, //Skinmesh
			/*
			&CameraReflInstance, //Skybox
			&CameraReflInstance, //Skycolor
			&CameraReflInstance, //SoundEffect
			&CameraReflInstance, //SoundSource
			&CameraReflInstance, //C++ source
			*/
			{SpriteReflInstance.ObjectUUID, &SpriteReflInstance}, //Sprite
		};
	}

	//populate the dictionaries
	for ( int i = IDB_PNG1, j = 0, k = 1 ; i <= IDB_PNG24 && j < Objects.size(); i++, j++, k++)
	{
		auto image = MAKEINTRESOURCEW(i);

		if (Objects[j].second == nullptr) continue;
		
		int categoryID = 0;

		if(categories.find(Objects[j].second->Category) == categories.end())
		{
			categories[Objects[j].second->Category] = k++;
			categoryID = k;
			addCategory(Objects[j].second->Category, categoryID);
		}
		else
		{
			categoryID = categories[Objects[j].second->Category];
		}
		
		objects.insert(std::make_pair(std::string(Objects[j].second->Name), objectItem{image, categoryID}));
	}

	//Populate the image list with the object data (icons and names)
	for (auto& [name, obj] : objects)
	{
		HBITMAP img = loadImgResource(obj.image, L"PNG", static_cast<HMODULE>(hInstance));
		int imgIndex = ImageList_Add(imageList, img, nullptr);
		DeleteObject(img);

		addObject(name, imgIndex, obj.categoryID );
	}

	resize();
}

void AddObjectDialog::resize()
{
	RECT r;
	GetClientRect(window, &r);
	int cx = r.right - r.left;
	int cy = r.bottom - r.top;
	SetWindowPos(listView, nullptr, 0, 0, cx, cy, SW_NORMAL);
}

AddObjectDialog::~AddObjectDialog()
{
	if (imageList != nullptr)
	{
		ImageList_Destroy(imageList);
		imageList = nullptr;
	}
}
