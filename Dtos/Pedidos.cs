namespace AluguelQuadras.Dtos;

// Formato do JSON que o usuário envia. Só tem os campos que ele pode informar.
// Id, Disponivel, ValorTotal e datas são decididos pela API, não pelo usuário.
public record NovaQuadra(string Nome, string Esporte, double ValorHora);

public record NovoCliente(string Nome, string Telefone);

public record NovoAluguel(int QuadraId, int ClienteId, int Horas);
