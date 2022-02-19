import os
import re

possible_result = [] # 记录可能的结果

for root, directories, files in os.walk("."):
    for file in files:
        if "Map" in file: # 礼包码只能出现在 MapXX.json 内
            with open(file, encoding="utf-8") as file_map:
                text = file_map.read()
                if "礼包码" not in text:
                    continue # 如果文件里不含礼包码，就跳过
                print(file, "可能含有礼包码")
                result = re.findall("\d{8}", text) # 礼包码只能是8位数字
                for i in result:
                    possible_result.append(i)

print("\n可能的礼包码：")
print(possible_result)
