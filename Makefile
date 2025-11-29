build:
	@echo -e "\033[1;36m[ Make ] \033[1;37mBuilding main app...\033[0m" && \
	dotnet publish -c Release -o . && \
	chmod +x sharpy && \
	sudo mv sharpy /usr/local/bin/sharpy && \
	echo -e "\033[1;36m[ Make ] \033[1;37mCleaning leftovers...\033[0m" && \
	cd - && \
	rm -rf sharpy Sharpy && \
	echo -e "\033[1;36m[ Make ] \033[1;37mBuild finished. Run using 'sharpy <-d/--debug>'\033[0m"
