.PHONY: publish deploy run clean status logs

PROJECT := ServerWatchAgent.csproj
DEPLOY_SCRIPT := ./scripts/deploy-grey-area.sh

publish:
	dotnet publish $(PROJECT) \
		-c Release \
		-r linux-x64 \
		--self-contained true \
		-o publish

deploy:
	$(DEPLOY_SCRIPT)

run:
	dotnet run --urls http://0.0.0.0:5189

clean:
	rm -rf publish
	dotnet clean

status:
	curl http://192.168.0.126:5189/status | jq

logs:
	ssh steve@192.168.0.126 \
		"sudo journalctl -u serverwatch-agent -n 50 --no-pager"