from os import listdir

def find_versions(build_type):
	lst = listdir('static')
	lst = [file for file in lst if build_type in file]
	return lst

def find_beta_versions():
	versions = find_versions('beta')
	return versions

def find_release_versions():
	versions = find_versions('release')
	return versions

def find_latest(versions):
	versions = sorted(versions, reverse=True)
	if len(versions) == 0:
		return []

	version = versions[0].split(".")
	length = len(version)
	del version[min(3, length):length]
	version.append(versions[0])
	return version;
