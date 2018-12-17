#!/bin/sh

build() {
  cmake -H. -Bbuild
  make -C build
}

run() {
  build/fractal
}

clean() {
  rm -rf build
}

if [ "$1" = "build" ]; then
  build
elif [ "$1" = "run" ]; then
  run
elif [ "$1" = "clean" ]; then
  clean
elif [ "$1" = "rebuild" ]; then
  clean
  build
elif [ "$1" = "" ]; then
  build
  run
else
  echo "Invalid argument. Listing valid arguments:\nTODO"
fi