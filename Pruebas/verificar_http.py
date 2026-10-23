import json, urllib.request, urllib.parse, urllib.error, re, hashlib, concurrent.futures
from pathlib import Path
base='http://localhost:5313'; front='http://localhost:5314'; count=0
def check(ok,msg):
    global count
    count+=1
    assert ok,msg
def call(path,data=None,server=base,raw=False,headers=None):
    if data is not None and not isinstance(data,bytes):data=json.dumps(data).encode()
    req=urllib.request.Request(server+path,data=data,headers=headers or {'Content-Type':'application/json'})
    try:
        with urllib.request.urlopen(req,timeout=40) as r:
            b=r.read();return r.status,b if raw else json.loads(b)
    except urllib.error.HTTPError as e:return e.code,e.read()
def nit(n):
    s=str(n);v=(11-sum(int(x)*(len(s)-i+1) for i,x in enumerate(s))%11)%11
    return s+('K' if v==10 else str(v))
a,b=nit(1234567),nit(7654321)
def factura(ref,**kw):
    x=dict(TIEMPO='Guatemala, 15/01/2025 15:25 hrs.',REFERENCIA=ref,NIT_EMISOR=a,NIT_RECEPTOR=b,VALOR=100,IVA=12,TOTAL=112);x.update(kw);return x
check(call('/api/salud')[0]==200,'salud')
check(call('/api/inicializar',b'')[0]==200,'inicializar')
s,r=call('/api/rtu',[{'NIT':a,'nombre':'Empresa Águila & Hijos'},{'NIT':b,'nombre':'Comercial Norte'},{'NIT':'123','nombre':'Inválido'}])
check(s==200 and r==dict(CONTRIBUYENTES_NUEVOS=2,CONTRIBUYENTES_ACTUALIZADOS=0,NITS_INVALIDOS=1),'rtu exacto')
s,r=call('/api/autorizaciones',[factura('A1'),factura('A2',VALOR=200,IVA=24,TOTAL=224)])
check(s==200,'autorizaciones')
ra=r['resultadoAutorizaciones'];check(set(ra)=={'cantidadAutorizacionesAprobadas','cantidadAutorizacionesRechazadasPorNitEmisorInexistente','cantidadAutorizacionesRechazadasPorNitReceptorInexistente','detalleAprobaciones'},'contrato exacto')
check(ra['cantidadAutorizacionesAprobadas']==2 and ra['detalleAprobaciones'][1]['codigoAprobacion']-ra['detalleAprobaciones'][0]['codigoAprobacion']==1,'correlativos')
for invalid in [{},[None],['abc'],[1],None]:
    check(call('/api/autorizaciones',json.dumps(invalid).encode())[0]==400,'estructura inválida')
check(call('/api/rtu',b'[mal')[0]==400,'json inválido')
check(call('/api/autorizaciones',[])[1]['resultadoAutorizaciones']['cantidadAutorizacionesAprobadas']==0,'vacío')
cases=[factura('A1'),factura('INV-E',NIT_EMISOR='x'),factura('INV-R',NIT_RECEPTOR='x'),factura('NO-E',NIT_EMISOR=nit(999)),factura('NO-R',NIT_RECEPTOR=nit(998)),factura('IVA',IVA=11,TOTAL=111),factura('TOTAL',TOTAL=12),factura('FECHA',TIEMPO='29/02/2025 10:00'),factura('NEG',VALOR=-1),factura('PREC',VALOR=1.001),factura('X'*41),factura('NULO',IVA=None),factura('HUGE',VALOR=1e100)]
check(call('/api/autorizaciones',cases)[1]['resultadoAutorizaciones']['cantidadAutorizacionesAprobadas']==0,'rechazos mixtos')
def reporte(tipo,**args):
    params=dict(desde='2025-01-15',hasta='2025-01-16',nit=a,direccion='emitidas');params.update(args)
    return call('/api/reportes/'+tipo+'?'+urllib.parse.urlencode(params))
check(reporte('rango')[1]['total']==336,'total rango')
check(reporte('rango',sinIva='true')[1]['total']==300,'rango sin IVA')
check(reporte('contribuyente')[1]['filas'][0]['celdas'][1]=='2','cantidad por fecha')
check(reporte('contribuyente',nit=b,direccion='recibidas')[1]['total']==336,'receptor')
check(reporte('facturas')[1]['filas'][1]['celdas'][1]=='A2','listado')
check(reporte('iva')[1]['filas'][0]['serie1']==36,'iva emisor')
check(reporte('iva')[1]['filas'][1]['serie2']==36,'iva receptor')
check(reporte('estadisticas')[1]['filas'][0]['celdas'][8]=='1','duplicadas')
for args in [dict(desde='mal'),dict(hasta='2020-01-01'),dict(nit='x'),dict(direccion='otra')]:
    check(reporte('contribuyente',**args)[0]==400,'filtro inválido')
with concurrent.futures.ThreadPoolExecutor(max_workers=8) as pool:
    results=list(pool.map(lambda i:call('/api/autorizaciones',[factura('C'+str(i))]),range(24)))
codes=[]
for s,r in results:
    check(s==200 and r['resultadoAutorizaciones']['cantidadAutorizacionesAprobadas']==1,'http concurrente')
    codes.append(r['resultadoAutorizaciones']['detalleAprobaciones'][0]['codigoAprobacion'])
check(len(set(codes))==24,'unicidad concurrente HTTP')
for page in ['/','/Enviar','/Peticiones','/Ayuda','/Inicializar']:
    s,html=call(page,server=front,raw=True);check(s==200 and b'<html' in html,page)
params='?consultar=true&Desde=2025-01-15&Hasta=2025-01-16&Nit='+a
out=Path('tmp/proyecto3');out.mkdir(exist_ok=True,parents=True)
for tipo in ['contribuyente','facturas','iva','rango','estadisticas']:
    s,h=call('/Peticiones'+params+'&Tipo='+tipo,server=front,raw=True)
    check(s==200 and b'Descargar PDF' in h,'vista '+tipo)
    if tipo in ['iva','rango']:check(b'<svg' in h,'gráfica '+tipo)
    s,pdf=call('/Peticiones'+params+'&pdf=true&Tipo='+tipo,server=front,raw=True)
    check(s==200 and pdf.startswith(b'%PDF'),'pdf '+tipo);(out/('reporte-'+tipo+'.pdf')).write_bytes(pdf)
check(call('/Peticiones?consultar=true&Tipo=rango&Desde=2025-01-15&Hasta=2025-01-16&Nit=',server=front,raw=True)[0]==200,'nit vacío en rango')
check(call('/Inicializar',b'Confirmar=true',server=front,raw=True,headers={'Content-Type':'application/x-www-form-urlencoded'})[0]==400,'antifalsificación')
print('HTTP correctas:',count)
(out/'http-resultado.txt').write_text(str(count),encoding='utf-8')
