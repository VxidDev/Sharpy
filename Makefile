build:
	@echo -e "\033[1;36m[ Make ] \033[1;37mCreating C# project...\033[0m" && \
	dotnet new console -n sharpy && \
	echo -e "\033[1;36m[ Make ] \033[1;37mImporting Sharpy code...\033[0m" && \
	mv Program.cs sharpy/Program.cs && \
	echo -e "\033[1;36m[ Make ] \033[1;37mBuilding main app...\033[0m" && \
	cd sharpy && dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o . && \
	chmod +x sharpy && \
	sudo mv sharpy /usr/local/bin/sharpy && \
	echo -e "\033[1;36m[ Make ] \033[1;37mCleaning leftovers...\033[0m" && \
	cd - && \
	rm -rf sharpy Sharpy && \
	echo -e "\033[1;36m[ Make ] \033[1;37mBuild finished. Run using 'sharpy <-d/--debug>'\033[0m"
