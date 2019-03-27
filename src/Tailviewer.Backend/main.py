"""`main` is the top level module for your Flask application."""

# Import the Flask Framework
from flask import Flask
from flask import Response
from flask import request
from flask import render_template
from flask import make_response
from versions import find_latest
from versions import find_beta_versions
from versions import find_release_versions

app = Flask(__name__)
# Note: We don't need to call run() since our application is embedded within
# the App Engine WSGI application server.


@app.route('/')
def hello():
	"""Return a friendly HTTP greeting."""
	return 'Hello World!'

@app.route('/query_version')
def check_version():
	latest_beta = find_latest(find_beta_versions())
	latest_release = find_latest(find_release_versions())
	template = render_template('versions.xml', beta=latest_beta, release=latest_release)
	response = make_response(template)
	response.headers['Content-Type'] = 'application/xml'
	return response

@app.errorhandler(404)
def page_not_found(e):
	"""Return a custom 404 error."""
	return 'Sorry, Nothing at this URL.', 404

@app.errorhandler(500)
def application_error(e):
	"""Return a custom 500 error."""
	return 'Sorry, unexpected error: {}'.format(e), 500
