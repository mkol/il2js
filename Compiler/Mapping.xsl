<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:i="http://smp.if.uj.edu.pl/~mkol/il2js/Mapping.xsd"
								>
	<xsl:template name="TypeFullName">
		<xsl:param name="node"/>
		<xsl:choose>
			<xsl:when test="$node/..=/">
				<xsl:value-of select="$node/@name"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="TypeFullName">
					<xsl:with-param name="node" select="$node/.."/>
				</xsl:call-template>.<xsl:value-of select="$node/@name"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="/">
		<html>
			<head>
				<title>Supported .NET types</title>
				<style>
					table{width:100%}
				</style>
			</head>
			<body>
				<xsl:apply-templates>
					<xsl:sort select="@name"/>
				</xsl:apply-templates>
			</body>
		</html>
	</xsl:template>

	<xsl:template match="i:namespace">
		<xsl:apply-templates select="i:type">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
		<xsl:apply-templates select="i:namespace">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="i:type">
		<h1>
			<xsl:call-template name="TypeFullName">
				<xsl:with-param name="node" select="."/>
			</xsl:call-template>
		</h1>
		<xsl:if test="i:constructor">
			<h2>Constructors</h2>
			<table rules="all" border="1">
				<tr>
					<th>.NET Signature</th>
					<th>JavaScript equivalent</th>
				</tr>
				<xsl:apply-templates select="i:constructor"/>
			</table>
		</xsl:if>
		<xsl:if test="i:method">
			<h2>Methods</h2>
			<table rules="all" border="1">
				<tr>
					<th>.NET Signature</th>
					<th>JavaScript equivalent</th>
				</tr>
				<xsl:apply-templates select="i:method">
					<xsl:sort select="@name"/>
				</xsl:apply-templates>
			</table>
		</xsl:if>
		<xsl:if test="i:operator">
			<h2>Operators</h2>
			<table rules="all" border="1">
				<tr>
					<th>.NET Signature</th>
					<th>JavaScript equivalent</th>
				</tr>
				<xsl:apply-templates select="i:operator">
					<xsl:sort select="@name"/>
				</xsl:apply-templates>
			</table>
		</xsl:if>
		<xsl:if test="i:property">
			<h2>Properties</h2>
			<table rules="all" border="1">
				<tr>
					<th colspan="2">.NET Signature</th>
					<th>JavaScript equivalent</th>
				</tr>
				<xsl:apply-templates select="i:property">
					<xsl:sort select="@name"/>
				</xsl:apply-templates>
			</table>
		</xsl:if>
	</xsl:template>

	<xsl:template match="i:constructor">
		<tr>
			<td>
				<code>
					<xsl:value-of select="../@name"/>
					(
					<xsl:apply-templates select="i:parameters/i:type" mode="parameter"/>
					)
				</code>
			</td>
			<td>
				<xsl:apply-templates select="i:javascript"/>
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="i:method|i:operator">
		<tr>
			<td>
				<code>
					<xsl:value-of select="@name"/>
					<xsl:if test="i:parameters">
						(
						<xsl:apply-templates select="i:parameters/i:type" mode="parameter"/>
						)
					</xsl:if>
				</code>
			</td>
			<td>
				<xsl:apply-templates select="i:javascript"/>
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="i:property">
		<tr>
			<td>
				<code>
					<xsl:value-of select="@name"/>

					<xsl:if test="i:parameters">
						(
						<xsl:apply-templates select="i:parameters/i:type" mode="parameter"/>
						)
					</xsl:if>
				</code>
			</td>
			<xsl:choose>
				<xsl:when test="i:get">
					<td>
						<code>{get}</code>
					</td>
					<td>
						<xsl:apply-templates select="i:get/i:javascript"/>
					</td>
				</xsl:when>
				<xsl:when test="i:set">
					<td>
						<code>{set}</code>
					</td>
					<td>
						<xsl:apply-templates select="i:set/i:javascript"/>
					</td>
				</xsl:when>
			</xsl:choose>
		</tr>
		<xsl:if test="i:get and i:set">
			<tr>
				<td></td>
				<td>
					<code>{set}</code>
				</td>
				<td>
					<xsl:apply-templates select="i:set/i:javascript"/>
				</td>
			</tr>
		</xsl:if>
	</xsl:template>

	<xsl:template match="i:type" mode="parameter">
		<xsl:value-of select="@name"/>
		<xsl:if test="position()!=last()">, </xsl:if>
	</xsl:template>

	<xsl:template match="i:javascript">
		<xsl:choose>
			<xsl:when test="i:method">
				<code>
					<xsl:value-of select="i:method/@name"/>
				</code> JavaScript function.
			</xsl:when>
			<xsl:when test="i:ignore">
				Method is ignored.
			</xsl:when>
			<xsl:when test="i:opCode">
				<code>
					<xsl:value-of select="i:opCode/@value"/>
				</code> IL operation code.
			</xsl:when>
			<xsl:when test="i:code">
				<code>
					<xsl:value-of select="i:code"/>
				</code> JavaScript code.
			</xsl:when>
		</xsl:choose>
	</xsl:template>
</xsl:stylesheet>
