"""Generate native Civil NX pairs in a currently EMPTY test model (licensed MAPI).
Usage: python generate_identical_pairs.py <private-config.json> <NEW-output-folder>
Config keys: baseUrl, mapiKey. Never commit credentials. HTTPS requests reject redirects.
Creates six DBUSER sections and twelve cantilevers; saves, solves, exports JSON/MCT.
Reads back all data; does not close/reset/overwrite another model or an output directory.
The caller must use the same host filesystem as Civil NX for save/export paths.
"""
import json, urllib.request, urllib.error, sys
from pathlib import Path
config=json.loads(Path(sys.argv[1]).resolve(strict=True).read_text())
class NoRedirect(urllib.request.HTTPRedirectHandler):
 def redirect_request(self,*args): raise RuntimeError('Unexpected redirect rejected')
opener=urllib.request.build_opener(NoRedirect())
def api(path,payload=None):
 request=urllib.request.Request(config['baseUrl'].rstrip('/')+'/'+path,
  data=None if payload is None else json.dumps(payload).encode(),
  headers={'MAPI-Key':config['mapiKey'],'Content-Type':'application/json'},method='GET' if payload is None else 'POST')
 with opener.open(request,timeout=55) as response: result=json.load(response)
 if 'error' in result: raise RuntimeError(path+': '+str(result['error'])[:200])
 return result
target=Path(sys.argv[2]).resolve()
target.mkdir(exist_ok=False)
for table in ['NODE','ELEM','SECT','MATL','STLD']:
 data=api('db/'+table)
 if data not in ({},{'message':''}): raise RuntimeError('Expected empty native test model; stop before writes: '+table)
unit=api('db/UNIT')
if any(unit['UNIT']['1'][k].upper()!=v for k,v in {'FORCE':'KN','DIST':'M','TEMPER':'C'}.items()):
 raise RuntimeError('Expected original kN/m/C units; no units changed')
sections={}
for shape,h,b,label in [('2C',.2,.075,'C'),('2L',.15,.1,'L-long'),('2L',.1,.15,'L-short')]:
 for gap in [.01,.03]:
  sections[str(len(sections)+1)]={'SECTTYPE':'DBUSER','SECT_NAME':f'GPC_PAIR_QA_{label}_{round(gap*1000)}',
   'SECT_BEFORE':{'OFFSET_PT':'CC','OFFSET_CENTER':0,'USER_OFFSET_REF':0,'HORZ_OFFSET_OPT':0,'USERDEF_OFFSET_YI':0,
    'VERT_OFFSET_OPT':0,'USERDEF_OFFSET_ZI':0,'USE_SHEAR_DEFORM':True,'USE_WARPING_EFFECT':False,'SHAPE':shape,'DATATYPE':2,
    'SECT_I':{'vSIZE':[h,b,.01,.012,gap,0,0,0,0,0]}}}
(target/'request-sections.json').write_text(json.dumps({'Assign':sections},indent=2))
created=api('db/SECT',{'Assign':sections})
print('Created section response keys:',','.join(created.keys()))
for table in ['UNIT','SECT']:
 (target/(table+'.json')).write_text(json.dumps(api('db/'+table),indent=2))
result=api('post/table',{'Argument':{'TABLE_NAME':'GPC_PAIR_QA','TABLE_TYPE':'SECTIONALL',
 'UNIT':{'FORCE':'N','DIST':'mm'},'STYLES':{'FORMAT':'Scientific','PLACE':12}}})
(target/'SECTIONALL.json').write_text(json.dumps(result,indent=2))
for table in result.values():
 if isinstance(table,dict) and 'HEAD' in table:
  cols=[table['HEAD'].index(k) for k in ['ID','Area','Iyy','Izz']]
  for row in table['DATA']: print('A/I:',[row[i] for i in cols])
out=target/'packed-pairs.mct'
result=api('doc/EXPORTMXT',{'Argument':str(out)})
(target/'export-response.json').write_text(json.dumps(result,indent=2))
print('Native MCT exists:',out.exists())

sections=api('db/SECT').get('SECT',{})
expected=json.loads((target/'SECT.json').read_text())['SECT']
if sections != expected: raise RuntimeError('Native test sections have changed; no writes')
for table in ['NODE','ELEM','MATL','STLD']:
 if api('db/'+table) not in ({},{'message':''}): raise RuntimeError('Expected empty '+table+' before appending QA model')
nodes,elements,supports,loads={},{},{},{}
for sect in range(1,7):
 for angle in (0,90):
  n=len(elements)+1;i=2*n-1;j=2*n
  nodes[str(i)]={'X':0,'Y':float(n),'Z':0}
  nodes[str(j)]={'X':4,'Y':float(n),'Z':0}
  elements[str(n)]={'TYPE':'BEAM','MATL':1,'SECT':sect,'NODE':[i,j,0,0,0,0,0,0],'ANGLE':angle,'STYPE':0}
  supports[str(i)]={'ITEMS':[{'ID':1,'GROUP_NAME':'','CONSTRAINT':'1111110'}]}
  loads[str(j)]={'ITEMS':[{'ID':1,'LCNAME':'GPC_PAIR_QA','GROUP_NAME':'','FX':-10,'FY':.2,'FZ':.3,'MX':.007,'MY':.004,'MZ':-.006}]}
requests={'MATL':{'1':{'TYPE':'STEEL','NAME':'S355','PARAM':[{'P_TYPE':1,'STANDARD':'EN05(S)','CODE':'','DB':'S355','bELAST':False,'ELAST':210000000}]}},
 'NODE':nodes,'ELEM':elements,'STLD':{'1':{'NO':1,'NAME':'GPC_PAIR_QA','TYPE':'USER','DESC':'Six-component cantilever benchmark'}},
 'CONS':supports,'CNLD':loads}
for table,data in requests.items():
 (target/('request-'+table+'.json')).write_text(json.dumps({'Assign':data},indent=2))
 api('db/'+table,{'Assign':data}); print('Created',table,len(data),flush=True)
model=target/'packed-pairs.mcb'
if model.exists(): raise RuntimeError('New model output already exists')
api('doc/SAVEAS',{'Argument':str(model)})
print('Saved native model',model.exists(),flush=True)
api('doc/ANAL',{})
print('Analysis response received',flush=True)
for table in ['UNIT','NODE','ELEM','MATL','SECT','STLD','CONS','CNLD']:
 (target/(table+'.json')).write_text(json.dumps(api('db/'+table),indent=2))
result=api('post/table',{'Argument':{'TABLE_NAME':'GPC_PAIR_FORCES','TABLE_TYPE':'BEAMFORCE','UNIT':{'FORCE':'N','DIST':'mm'},
 'STYLES':{'FORMAT':'Scientific','PLACE':12},'NODE_ELEMS':{'KEYS':list(range(1,13))},'LOAD_CASE_NAMES':['GPC_PAIR_QA(ST)']}})
(target/'BEAMFORCE.json').write_text(json.dumps(result,indent=2))
result=api('post/table',{'Argument':{'TABLE_NAME':'GPC_MATERIAL','TABLE_TYPE':'MATERIAL','UNIT':{'FORCE':'N','DIST':'mm'}}})
(target/'MATERIAL.json').write_text(json.dumps(result,indent=2))
out=target/'packed-pairs-full.mct'
if out.exists(): raise RuntimeError('New MCT output already exists')
api('doc/EXPORTMXT',{'Argument':str(out)})
print('Saved full native MCT',out.exists(),flush=True)

for table in ['THIK','GRUP','BNGR','LDGR','SKEW','OFFS','FRLS','ELNK','RIGD','STAG','STCT','BMLD','PRES','BODF','NBOF','LCOM-GEN','LCOM-CONC','LCOM-STEEL','LCOM-SRC','LCOM-STLCOMP','LCOM-SEISMIC']:
 (target/(table+'.json')).write_text(json.dumps(api('db/'+table),indent=2))
print('Native fixture captured. Do not overwrite an existing checked-in fixture without reviewing the differences.')
