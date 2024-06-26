project (MklProxyNative)

set(SOURCES
    MklProxyNative.cpp
)

find_library(MKL_LIBRARY MklImports HINTS ${MKL_LIB_PATH})
if(NOT WIN32)
    list(APPEND SOURCES ${VERSION_FILE_PATH})
    if(NOT APPLE)
        SET(CMAKE_SKIP_BUILD_RPATH  FALSE)
        SET(CMAKE_BUILD_WITH_INSTALL_RPATH FALSE)
        SET(CMAKE_INSTALL_RPATH_USE_LINK_PATH TRUE)
        SET(CMAKE_INSTALL_RPATH "$ORIGIN/")
    endif()
endif()

add_library(MklProxyNative SHARED ${SOURCES} ${RESOURCES})
target_link_libraries(MklProxyNative PUBLIC ${MKL_LIBRARY})

if (MSVC AND NOT MSVC_VERSION LESS 1900)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /guard:cf")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /Qspectre")
endif()

if(APPLE)
    set_target_properties(MklProxyNative PROPERTIES INSTALL_RPATH "@loader_path")
endif()

install_library_and_symbols (MklProxyNative)