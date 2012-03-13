//il2js Compiler - JavaScript VM for .NET
//Copyright (C) 2012 Michael Kolarz

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.
var $path$ = $pathValue$,
//#if guid
$guid$=$guidValue$,
//#endif
$method$={},
//ilosc argumentow
$methodLength$={},
$field$={},
$ctor$={0:Class.create(),1:Array},
$string$={},
$code$={},
//#if design
$HandlingType={Throw$:0,Leave$:1},
$native$=$string$,
$OpCode={},
//#endif
$loaded$={},
$cctor_executed$={},
$module$={};
//#if IE
function $apply$(m,t,a){
	return t[m].apply?
		t[m].apply(t,a):
		eval("t[m]("+$R(0,a.length,true).map(function(x){return"a["+x+"]";}).join()+")");
}
//#endif
function $getNativeMethod$(i,j){
	return Object.isFunction(j=$code$[i])?
		j:
		$code$[i]=
			j.startsWith("function")?			
				eval(
//#if IE
					"j=("+
//#else									
					"("+
//#endif					
					j+")"):
				Function("return "+j.replace(
					/\$(\*|[0-9]+)/g,
					function(y,z){
						return"arguments"+(z=="*"?"":"["+z+"]");
					}
				));
};
//#region delegate
//o i m sa uzywane w rzeczach .netowych
//to jest masowo uzywane w nativeattribute
//@param b symbol watka
//@param c zmienna lokalna
function $handler$(a,b,c){
	c=function(){
		return $call$(a.m,$A(arguments),b);
	};
	return a.o==null?c:c.curry(a.o);
};
//#endregion
//#region execution
function $execution$(a,b){
	return{
		$local$:[],
		$position$:0,
		$args$:b,
		$il$:$method$[a],
		$tryStack$:[],
		$handlingStack$:[]
	};
};
function $eatInt$(_){
	return _.$il$.charAt(_.$position$)=="-"?(++_.$position$,-$eatUInt$(_)):$eatUInt$(_);
};
function $eatUInt$(a,b){
	b=a.$position$;
	do++a.$position$;while(/[0-9a-f]/.test(a.$il$.charAt(a.$position$)));
//#if debug
	return $debug$.eaten=parseInt(a.$il$.substring(b,a.$position$),16);
//#else
	return parseInt(a.$il$.substring(b,a.$position$),16);
//#endif
};
//#if design
function $eatToken$(_){return $eatChar$(_);};
//#endif	
function $eatChar$(_){return _.$il$.charAt(_.$position$++);};
//#endregion
//#region $ref$
//to jest duzo uzywane w mapping2 wiec nie warto z tego rezygnowac
function $set$(b,a){
	b.$ref_container$[b.$ref_index$]=a;
};
//#endregion
//uwaga: t jest uzywane w methodcompiler-js stack machine.cs w thread_currentthread
function $call$($callStack$,$stack$,t,_,a,b,c,d){
	if(!_){
	//uwaga: to co jest wykonywane nie jest na stosie
		_=$execution$($callStack$,$stack$);

		$callStack$=[];
		$stack$=[];
		
		//t|| jest istotne bo $handler$ to ustawia (patrz Mapping2.xml)
		
		t=t||{};
	}

	function $pop$(){
	//#if debug
		return $debug$.pop=$stack$.pop();
	//#else
		return $stack$.pop();
	//#endif
	};
	function $pops$(c,r){
		r=$stack$.slice($stack$.length-c,$stack$.length);
		$stack$.length-=c;
		return r;
	};
	function $push$(a){
		$stack$.push(a);
	};


	function $operator$(a,b){
		b=$pop$();
		$push$(eval("$pop$()"+a+"b"));
	};
	function $br2$(o,p,y){
		p=$eatInt$(_);
		y=$pop$();
		if(eval("$pop$()"+o+"y"))
			_.$position$+=p;
	};
		
	do while(_.$position$<_.$il$.length)try{
		switch($eatChar$(_)){
			case $OpCode.Leq$:$operator$("<=");break;
			case $OpCode.Geq$:$operator$(">=");break;
			case $OpCode.Ceq$:$operator$("==");break;
			case $OpCode.Blt$:$br2$("<");break;
			case $OpCode.Bgt$:$br2$(">");break;
			case $OpCode.Ble$:$br2$("<=");break;
			case $OpCode.Bge$:$br2$(">=");break;
			case $OpCode.Beq$:$br2$("==");break;
			case $OpCode.Bne$:$br2$("!=");break;
			case $OpCode.Ret$:if(!(_=$callStack$.pop()))return $pop$();break;
			case $OpCode.Pop$:$pop$();break;
			case $OpCode.Ldnull$:$push$(null);break;
			case $OpCode.Lds$:$push$($string$[$eatUInt$(_)]);break;
			case $OpCode.StL$:_.$local$[$eatUInt$(_)]=$pop$();break;
			case $OpCode.LdL$:$push$(_.$local$[$eatUInt$(_)]);break;
			case $OpCode.StA$:_.$args$[$eatUInt$(_)]=$pop$();break;
			case $OpCode.LdA$:$push$(_.$args$[$eatUInt$(_)]);break;
			case $OpCode.Ldlen$:$push$($pop$().length);break;
			case $OpCode.LdI$:$push$($eatInt$(_));break;
			case $OpCode.Ldc_R$:$push$(parseFloat($string$[$eatUInt$(_)]));break;
			//taka kolejnosc bo eatint zmienia position. a+=b === a=a+b co jest zle tutaj
			case $OpCode.Br$:_.$position$=$eatInt$(_)+_.$position$;break;
			case $OpCode.Ldfld$:$push$($pop$()[$eatToken$(_)]);break;
			case $OpCode.Ldsfld$:$push$($field$[$eatUInt$(_)]);break;
			case $OpCode.Stsfld$:$field$[$eatUInt$(_)]=$pop$();break;
			case $OpCode.Ldfun$:$push$($eatUInt$(_));break;
			case $OpCode.NLdf$:$push$($pop$()[$native$[$eatUInt$(_)]]);break;
			case $OpCode.Ldsfld_native$:$push$(eval($native$[$eatUInt$(_)]));break;
			case $OpCode.Arr$:$push$($R(1,$pop$()).map(function(x){return 0;}));break;
			case $OpCode.Dup$:$push$($stack$.last());break;
			case $OpCode.Conv_I$:$push$(parseInt($pop$())%(Math.pow(2,$eatUInt$(_)*8-1)));break;
			case $OpCode.Ldind_or_Constrained$:
				a=$pop$();
				$push$(a.$ref_container$[a.$ref_index$]);
				break;
			case $OpCode.Conv_U$:
				a=Math.pow(2,$eatUInt$(_)*8);
				$push$(((parseInt($pop$())%(a/2))+a)%a);
				break;
			case $OpCode.Isinst$:
				$push$((a=$pop$())instanceof $ctor$[$eatUInt$(_)]?a:null);
				break;
			case $OpCode.Isinst_native$:
				a=$pop$();
				b=$pop$();
				$push$(typeof b==a?b:null);
				break;
			case $OpCode.Stsfld_native$:
				a=$pop$();
				eval($native$[$eatUInt$(_)]+"=a");
				break;
			case $OpCode.Sw$:
				d=$eatUInt$(_);
				c=$pop$();
				a=[];
				for(b=0;b<d;++b){
					++_.$position$;
					a[b]=$eatUInt$(_);
				}
				if(c<d)_.$position$+=a[c];
				break;
			case $OpCode.Stelem$:
				b=$pop$();
				a=$pop$();
				$pop$()[a]=b;
				break;
			case $OpCode.Ldelem$:
				a=$pop$();
				$push$($pop$()[a]);
				break;
			case $OpCode.Brt$:
				a=$eatUInt$(_);
				if($pop$())
					_.$position$+=a;
				break;
			case $OpCode.Brf$:
				a=$eatUInt$(_);
				if(!$pop$())
					_.$position$+=a;
				break;
			case $OpCode.Stfld$:
				a=$pop$();
				$pop$()[$eatToken$(_)]=a;
				break;
			case $OpCode.Cl$:
				a=$pops$($eatUInt$(_));
				b=$pop$();
				$callStack$.push(_);
				_=$execution$(b.m,b.o==null?a:[b.o].concat(a));
				break;
			case $OpCode.New$:
				d=$eatUInt$(_);
				++_.$position$;
				b=$eatUInt$(_);
				$callStack$.push(_);
				_=$execution$(b,[d in $ctor$?new $ctor$[d]():{}].concat($pops$($methodLength$[b]-1)));
				break;
			case $OpCode.NNew$:
				d=$eatUInt$(_);
				++_.$position$;
				a=$pops$(c=$eatUInt$(_));
				$push$(eval("new "+$native$[d]+"("+$R(0,c,true).map(function(n){return"a["+n+"]"}).join()+")"));
				break;
			case $OpCode.Call$:
				d=$eatUInt$(_);
				a=$pops$($methodLength$[d]);
				if($eatChar$(_)==$OpCode.Tr$)
					a[0]=a[0].$ref_container$[a[0].$ref_index$];
				$callStack$.push(_);
				_=$execution$(d,a);
				break;
			case $OpCode.Callvirt$:
				d=$eatUInt$(_);
				a=$pops$($methodLength$[d]);
				if($eatChar$(_)==$OpCode.Tr$)
					d=a[0]["v"+d];
				d=a[0]["v"+d];
				$callStack$.push(_);
				_=$execution$(d,a);
				break;
			case $OpCode.NCs$:
				b=eval($native$[$eatUInt$(_)]);
				++_.$position$;
				a=$native$[$eatUInt$(_)];
				c=$eatChar$(_)==$OpCode.Tr$;
//#if IE
				d=$apply$(a,b,$pops$($eatUInt$(_)));
//#else
				d=b[a].apply(b,$pops$($eatUInt$(_)));
//#endif					
				if(c)$push$(d);
				break;
			case $OpCode.NCv$:
				b=$native$[$eatUInt$(_)];
				d=$eatChar$(_)==$OpCode.Tr$;
				a=$pops$($eatUInt$(_));
				c=$pop$();
				if($eatChar$(_)==$OpCode.Tr$)
					c=c.$ref_container$[c.$ref_index$];
//#if IE
				b=$apply$(b,c,a);
//#else						
				b=c[b].apply(c,a);
//#endif					
				if(d)$push$(b);
				break;
			case $OpCode.NativeCode$:
				a=$getNativeMethod$($eatUInt$(_));
				c=$eatChar$(_)==$OpCode.Tr$;
				a=a.apply(0,$pops$($eatUInt$(_)));
				if(c)$push$(a);
				break;
			case $OpCode.NCvc$:
				b=$getNativeMethod$($eatUInt$(_));
				d=$eatChar$(_)==$OpCode.Tr$;
				a=$pops$($eatUInt$(_));
				c=$pop$();
				if($eatChar$(_)==$OpCode.Tr$)
					c=c.$ref_container$[c.$ref_index$];
				b=b.apply(c,a);
				if(d)$push$(b);
				break;
			case $OpCode.NStf$:
				d=$pop$();
				$pop$()[$native$[$eatUInt$(_)]]=d;
				break;
			case $OpCode.Ca$:
				b=$eatUInt$(_);
				a=$eatChar$(_)==$OpCode.Tr$;
				d=new Ajax.Request($path$+
//#if guid
					$guid$+".ashx?"+
//#else				
					"$handlerFileName$.ashx?"+
//#endif					
					b,{postBody:Object.toJSON($pops$($eatUInt$(_))),asynchronous:false});
				b=eval(d.transport.responseText);
				if(a)$push$(b);
				break;
			case $OpCode.Css$:
				a=$pops$($eatUInt$(_));
				d=$eatChar$(_)==$OpCode.Tr$;
				c=$pop$();
				if(c.o!=null)
					a=[c.o].concat(a);
				a=new Ajax.Request($path$+
//#if guid
					$guid$+".ashx?"+
//#else				
					"$handlerFileName$.ashx?"+
//#endif					
					c.m,{postBody:Object.toJSON(a),asynchronous:false});
				b=eval(a.transport.responseText);
				if(d)$push$(b);
				break;
			case $OpCode.Csa$:
				c=$eatUInt$(_);
				d=$eatChar$(_)==$OpCode.Tr$;
				b=d?$pop$():null;
				a=$pops$(c);
				c=$pop$();
				if(c.o!=null)
					a=[c.o].concat(a);
				new Ajax.Request($path$+
//#if guid
					$guid$+".ashx?"+
//#else				
					"$handlerFileName$.ashx?"+
//#endif					
					c.m,{postBody:Object.toJSON(a),onSuccess:d?(function(b,x){
					$handler$(b)(eval(x.transport.responseText));
				}).curry(b):Prototype.K});
				break;
			case $OpCode.Stind$:
				a=$pop$();
				$set$($pop$(),a);
				break;
			case $OpCode.CallNativeCtor$:
				d=$eatUInt$(_);
				++_.$position$;
				a=$pops$(c=$eatUInt$(_));
				//usuwamy {}				
				$pop$();
				$push$(a=eval("new "+$native$[d]+"("+$R(0,c,true).map(function(n){return"a["+n+"]"}).join()+")"));
				for(b=$callStack$.length-1;$callStack$[b].$args$[0]==_.$args$[0];--b){
					$callStack$[b].$args$[0]=a;
				}
				_.$args$[0]=a;
				break;
			case $OpCode.LoadModule$:
				a=$eatUInt$(_);
				if($module$[a]!=1){
					new Ajax.Request($path$+$module$[a]+".js",{asynchronous:false,method:"get"});
					$module$[a]=1;
				}
				break;
//#region exceptions
			case $OpCode.BeginTry$:_.$tryStack$.push((a=$eatUInt$(_))+_.$position$);break;
			case $OpCode.Throw$:throw $pop$(_);break;
			case $OpCode.Leave$:
				//gdzie trzeba skoczyc
				a=$eatUInt$(_)+_.$position$;
				//oposzczenie bloku try{}
				if(a>_.$tryStack$.last()){
					//WTF? $stack$.length=0;
//tam jest koniec try{} wiec albo tu bedzie BeginCatch albo BeginFinally
					_.$position$=_.$tryStack$.pop();
					if($eatChar$(_)==$OpCode.BeginFinally$)
//czyli jest finally
						_.$handlingStack$.push({$Handling_Type$:$HandlingType.Leave$,$Handling_Argument$:a});
//czyli sa catch'e => nie ma finally (tak wynika z autopsji)					
					else _.$position$=a;
				}else{
				//jakies inne (np wyskoczenie z zagniezdzonych petli przez goto)}
					_.$position$=a;
				}
				break;
			case $OpCode.Endfinally$:
				a=_.$handlingStack$.pop();
				//if(a.$Handling_Type$==$HandlingType.Throw$
				if(a.$Handling_Type$)
					//czyli byl wczesniej throw
					throw a.$Handling_Argument$;
				else _.$position$=a.$Handling_Argument$;
					//czyli byl wczesniej leave
				break;
//#endregion //exceptions
			case $OpCode.Box_Unbox_Any$:
				a=$pop$();
				switch($eatChar$(_)){
					case $OpCode.Box_Number$:a=new Number(a);break;
					case $OpCode.Box_Boolean$:a=new Boolean(a);break;
					case $OpCode.Box_Char$:a=String.fromCharCode(a);break;
					case $OpCode.Unbox_Any_Number$:a=a+0;break;
					case $OpCode.Unbox_Any_Boolean$:a=a==true;break;
					//case $OpCode.Unbox_Any_Char$:
					default:a=a.charCodeAt(0);
				}
				$push$(a);
				break;
			case $OpCode.Ld_a$:
				switch($eatChar$(_)){
					case $OpCode.Ld_a_loc$:a=_.$local$;b=$eatUInt$(_);break;
					case $OpCode.Ld_a_arg$:a=_.$args$;b=$eatUInt$(_);break;
					case $OpCode.Ld_a_elem$:b=$pop$();a=$pop$();break;
					case $OpCode.Ld_a_fld$:a=$pop$();b=$eatToken$(_);break;
					//case $OpCode.Ld_a_fld_native$:
					default:a=$pop$();b=$native$[$eatUInt$(_)];
				}
				$push$({
					$ref_container$:a,
					$ref_index$:b
				});
				break;
			case $OpCode.Sleep$:
//#if IE
				t.t=setTimeout($call$.curry($callStack$,$stack$,t,_),$pop$());
//#else
				t.t=setTimeout($call$,$pop$(),$callStack$,$stack$,t,_);
//#endif					
				return;
//			case $OpCode.And$:$operator$("&");break;
//			case $OpCode.Or$:$operator$("|");break;
//			case $OpCode.Add$:$operator$("+");break;
//			case $OpCode.Xor$:$operator$("^");break;
//			case $OpCode.Sub$:$operator$("-");break;
//			case $OpCode.Div$:$operator$("/");break;
//			case $OpCode.Rem$:$operator$("%");break;
//			case $OpCode.Mul$:$operator$("*");break;
//			case $OpCode.Clt$:$operator$("<");break;
//			case $OpCode.Cgt$:$operator$(">");break;
			default:$operator$(_.$il$.charAt(_.$position$-1));
		}
	}catch(a){
		//to musi byc funkcja bo return.
		//jest wolana tylko w catch'u by sie nie petlilo rzucanie wyjatkow.
		(function(x){
			do if(_.$position$=_.$tryStack$.pop()){
					//czyli jestesmy w jakims try'y
					if($eatChar$(_)==$OpCode.BeginFinally$)
						//czyli jest finally
						_.$handlingStack$.push({$Handling_Type$:$HandlingType.Throw$,$Handling_Argument$:x});
					else{
						//czyli sa catch'e => nie ma finally (tak wynika z autopsji)
						//TODO
						//$eatUInt$(_);
						$push$(x);
					}
					return;
				}
			while(_=$callStack$.pop());
			//alert("Unhandled exception\n"+x);
			throw x;
		})(a);
	}
	while(_=$callStack$.pop());
	return $pop$();
};
function $init$(_,a,b,c,d,e,f,g,o,w,r,p,q,s,t,y,z,h,u,i,j,k){
	//zwykly parseInt
	function l(_){
		return parseInt(_,16);
	};
  //splitowanie z ewentualnim each`em
  function x(_,a){
		_=_.split("`");
		return a?_.each(a):_;
  };
	//inicjalizacja tablicy stringow
	function m(a,b,c){
		if(a){
			a=x(a);
			x(b,function(i,j){
				c[l(a[j])]=i;
			});
		}
	};
	//inicjalizacja pol statycznych
	function n(a,b){
		if(a)
			x(a,function(c){
				c=l(c);
				if(!(c in $field$))
					$field$[c]=b;
			});
	};
	if(!_||!$loaded$[i=_.location.pathname.toLowerCase()]){
		$loaded$[i]=1;
		if(a){
			b=x(b);
			x(a,function(i,j){
				i=l(i);
				$method$[i]=b[j];
				$methodLength$[i]=c[j];
			});
		}
		m(d,e,$string$);
		m(f,g,$code$);
		if(o)
			eval.call(window,o);
		n(w,0);
		n(r,null);
		if(p){
			for(i=0;2*i<p.length;++i){
				j=p[2*i];
				if(!$ctor$[j])
					$ctor$[j]=Class.create($ctor$[p[2*i+1]]);
				j=$ctor$[j].prototype;
				if(q[i])
					for(k in q[i])
						j["v"+k]=q[i][k];
			}
		}
		if(h){
			h=x(h);
			x(u,function(i,j){
				j=l(h[j]);
				if(!(j in $module$))
					$module$[j]=i;
			});
		}
		if(s){
			t=x(t);
			x(s,function(i,j){
				i=l(i);
				if(!$cctor_executed$[i]){
					$cctor_executed$[i]=1;
					$method$[0]=t[j];
					$call$(0,[]);
				}
			});
		}
	}
	if(z)
		_.document.observe("dom:loaded",$call$.curry(z,[_]));
	if(y)
		$call$(y,[_]);
};
//#if debug
var $debug$={};
//#endif
