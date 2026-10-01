.PHONY: fetch-refs build test verify package clean

ROOT := $(abspath $(dir $(lastword $(MAKEFILE_LIST))))
SOLUTION := $(ROOT)/EvuMinimap.sln

fetch-refs:
	bash "$(ROOT)/scripts/fetch-refs.sh"

build: fetch-refs
	dotnet build "$(SOLUTION)" -c Release

test: build
	dotnet test "$(SOLUTION)" -c Release --no-build

verify: test

package: build
	bash "$(ROOT)/scripts/package.sh"

clean:
	dotnet clean "$(SOLUTION)" -c Release
	rm -rf "$(ROOT)/dist"
